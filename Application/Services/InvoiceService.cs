using Application.Common;
using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappers;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using System.Globalization;

namespace Application.Services;

public class InvoiceService(
    IOrderRepository orderRepository,
    IInvoiceRepository invoiceRepository,
    IMenuItemRepository menuItemRepository,
    IInventoryRepository inventoryRepository,
    IMenuAddonRepository menuAddonRepository,
    IReceiptPrintingService receiptPrintingService,
    IPosTerminalService posTerminalService)
{
    public async Task<InvoiceResponse> CreateAsync(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Items is null || request.Items.Count == 0)
            throw new DomainException("An invoice must contain at least one item.");

        var invoice = Invoice.Create(
            GenerateInvoiceNumber(),
            request.Channel,
            request.DiscountAmount,
            request.TaxAmount);

        var itemGroups = request.Items.GroupBy(item => new
        {
            item.MenuItemId,
            AddonKey = BuildAddonKey(item.Addons)
        });

        foreach (var itemRequestGroup in itemGroups)
        {
            var menuItem = await menuItemRepository.GetByIdAsync(
                itemRequestGroup.Key.MenuItemId,
                cancellationToken)
                ?? throw new NotFoundException(
                    $"Menu item with id '{itemRequestGroup.Key.MenuItemId}' was not found.");

            if (!menuItem.IsAvailable)
                throw new DomainException($"Menu item '{menuItem.Name}' is not available.");

            var quantity = itemRequestGroup.Sum(item => item.Quantity);
            var invoiceItem = InvoiceItem.Create(
                menuItem.Id,
                menuItem.Name,
                quantity,
                menuItem.Price);

            var addonRequests = itemRequestGroup.First().Addons?
                .GroupBy(addon => addon.MenuAddonId)
                .Select(group => new
                {
                    MenuAddonId = group.Key,
                    Quantity = group.Sum(addon => addon.Quantity)
                })
                .ToList() ?? [];

            if (addonRequests.Count > 0)
            {
                var addons = await menuAddonRepository.GetByIdsAsync(
                    addonRequests.Select(addon => addon.MenuAddonId).ToList(),
                    cancellationToken);
                if (addons.Count != addonRequests.Count)
                    throw new NotFoundException("One or more menu add-ons were not found.");

                var addonMap = addons.ToDictionary(addon => addon.Id);
                foreach (var addonRequest in addonRequests)
                {
                    var addon = addonMap[addonRequest.MenuAddonId];
                    if (!addon.IsAvailable)
                        throw new DomainException(
                            $"Menu add-on '{addon.Name}' is not available.");

                    if (!await menuAddonRepository.IsApplicableToMenuItemAsync(
                            addon.Id,
                            menuItem.Id,
                            cancellationToken))
                    {
                        throw new DomainException(
                            $"Menu add-on '{addon.Name}' is not applicable to menu item '{menuItem.Name}'.");
                    }

                    invoiceItem.AddAddon(InvoiceItemAddon.Create(
                        addon.Id,
                        addon.Name,
                        addonRequest.Quantity * quantity,
                        addon.Price));
                }
            }

            invoice.AddItem(invoiceItem);
        }

        await invoiceRepository.AddAsync(invoice, cancellationToken);
        await invoiceRepository.SaveChangesAsync(cancellationToken);

        return invoice.ToResponse();
    }

    public async Task<InvoiceResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Invoice with id '{id}' was not found.");

        return invoice.ToResponse();
    }

    public async Task<InvoiceResponse> PayInvoiceAsync(Guid id, Guid userId)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Invoice with id '{id}' was not found.");
        invoice.Pay(userId);
        await invoiceRepository.SaveChangesAsync();
        return invoice.ToResponse();
    }

    public async Task<InvoiceResponse> CancelInvoiceAsync(Guid id, Guid userId)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Invoice with id '{id}' was not found.");
        invoice.Cancel(userId);
        await invoiceRepository.SaveChangesAsync();
        return invoice.ToResponse();
    }

    public async Task<InvoiceResponse> FinalizeAsync(
        Guid id,
        FinalizeInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Invoice with id '{id}' was not found.");

        var finalizedAtUtc = DateTime.UtcNow;
        invoice.Finalize(request.PaymentMethod, finalizedAtUtc);
        await ConsumeInventoryAsync(invoice, finalizedAtUtc, cancellationToken);
        await inventoryRepository.SaveChangesAsync(cancellationToken);

        var response = invoice.ToResponse();
        try
        {
            await receiptPrintingService.PrintAsync(
                invoice.Id,
                Domain.Enums.ReceiptType.Kitchen,
                cancellationToken);
        }
        catch (Application.Exceptions.PrintingException)
        {
            // Settlement is already durable; a failed printer must not leave
            // the invoice or inventory in an indeterminate state.
        }

        return response;
    }

    public async Task<PosPaymentResult> RequestCardPaymentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Invoice with id '{id}' was not found.");
        if (invoice.Status != InvoiceStatus.Draft)
            throw new DomainException("Only draft invoices can request card payment.");

        invoice.BeginPosPayment();
        await invoiceRepository.SaveChangesAsync(cancellationToken);

        PosPaymentResult result;
        try
        {
            result = await posTerminalService.RequestPaymentAsync(
                invoice.TotalAmount,
                invoice.InvoiceNumber,
                cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            invoice.FailPosPayment(PosPaymentState.TimedOut, "POS terminal request timed out.");
            await invoiceRepository.SaveChangesAsync(cancellationToken);
            return new(false, "TimedOut", ErrorMessage: "POS terminal request timed out.");
        }

        if (!result.Succeeded)
        {
            var state = result.Status.ToLowerInvariant() switch
            {
                "cancelled" => PosPaymentState.Cancelled,
                "timedout" => PosPaymentState.TimedOut,
                "unknown" => PosPaymentState.Unknown,
                _ => PosPaymentState.Failed
            };
            invoice.FailPosPayment(state, result.ErrorMessage);
            await invoiceRepository.SaveChangesAsync(cancellationToken);
            return result;
        }

        invoice.CompletePosPayment(result.ReferenceNumber
            ?? throw new DomainException("The POS terminal returned success without a reference number."));
        await invoiceRepository.SaveChangesAsync(cancellationToken);

        try
        {
            await FinalizeAsync(
                id,
                new FinalizeInvoiceRequest(PaymentMethod.Card),
                cancellationToken);
            return result;
        }
        catch (Exception exception) when (exception is DomainException or InventoryConcurrencyException)
        {
            var current = await invoiceRepository.GetByIdAsync(id, cancellationToken);
            if (current is not null)
            {
                current.MarkPaymentUnknown($"Payment succeeded but invoice settlement failed: {exception.Message}");
                await invoiceRepository.SaveChangesAsync(cancellationToken);
            }
            return new(false, "Unknown", result.ReferenceNumber,
                "Payment succeeded but invoice settlement could not be completed.");
        }
    }

    private async Task ConsumeInventoryAsync(
        Invoice invoice,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken)
    {
        var menuItemIds = invoice.Items
            .Select(item => item.MenuItemId)
            .Distinct()
            .ToList();
        var recipes = await inventoryRepository.GetRecipesByMenuItemIdsAsync(
            menuItemIds,
            cancellationToken);
        var recipeByMenuItemId = recipes.ToDictionary(recipe => recipe.MenuItemId);

        var missingRecipeItemIds = menuItemIds
            .Where(menuItemId => !recipeByMenuItemId.ContainsKey(menuItemId))
            .ToList();
        if (missingRecipeItemIds.Count > 0)
        {
            throw new DomainException(
                $"Cannot finalize the invoice because {missingRecipeItemIds.Count} menu item(s) do not have an inventory recipe.");
        }

        var addonIds = invoice.Items
            .SelectMany(item => item.Addons)
            .Select(addon => addon.MenuAddonId)
            .Distinct()
            .ToList();
        var addonRecipes = await inventoryRepository.GetRecipesByMenuAddonIdsAsync(
            addonIds,
            cancellationToken);
        var recipeByMenuAddonId = addonRecipes.ToDictionary(recipe => recipe.MenuAddonId);

        var missingRecipeAddonIds = addonIds
            .Where(menuAddonId => !recipeByMenuAddonId.ContainsKey(menuAddonId))
            .ToList();
        if (missingRecipeAddonIds.Count > 0)
        {
            throw new DomainException(
                $"Cannot finalize the invoice because {missingRecipeAddonIds.Count} menu add-on(s) do not have an inventory recipe.");
        }

        var menuItemRequirements = invoice.Items
            .SelectMany(item => recipeByMenuItemId[item.MenuItemId].Components.Select(
                component => new
                {
                    component.IngredientId,
                    Quantity = component.Quantity * item.Quantity
                }));
        var addonRequirements = invoice.Items
            .SelectMany(item => item.Addons)
            .SelectMany(addon => recipeByMenuAddonId[addon.MenuAddonId].Components.Select(
                component => new
                {
                    component.IngredientId,
                    Quantity = component.Quantity * addon.Quantity
                }));

        var requiredQuantities = menuItemRequirements
            .Concat(addonRequirements)
            .GroupBy(requirement => requirement.IngredientId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(requirement => requirement.Quantity));

        var ingredients = await inventoryRepository.GetIngredientsByIdsAsync(
            requiredQuantities.Keys.ToList(),
            cancellationToken);
        if (ingredients.Count != requiredQuantities.Count)
            throw new DomainException("One or more recipe ingredients no longer exist.");

        var insufficientIngredients = ingredients
            .Where(ingredient => ingredient.CurrentStock < requiredQuantities[ingredient.Id])
            .OrderBy(ingredient => ingredient.Name)
            .Select(ingredient =>
                $"{ingredient.Name} (required: {requiredQuantities[ingredient.Id]}, available: {ingredient.CurrentStock})")
            .ToList();
        if (insufficientIngredients.Count > 0)
        {
            throw new DomainException(
                $"Insufficient inventory: {string.Join("; ", insufficientIngredients)}.");
        }

        foreach (var ingredient in ingredients.OrderBy(ingredient => ingredient.Id))
        {
            var quantity = requiredQuantities[ingredient.Id];
            var balanceAfter = ingredient.Consume(quantity);
            await inventoryRepository.AddLedgerEntryAsync(
                StockLedgerEntry.Create(
                    ingredient.Id,
                    Domain.Enums.StockMovementType.InvoiceConsumption,
                    -quantity,
                    balanceAfter,
                    invoice.Id,
                    $"Invoice {invoice.InvoiceNumber}",
                    occurredAtUtc),
                cancellationToken);
        }
    }

    private static string BuildAddonKey(
        IReadOnlyList<CreateInvoiceItemAddonRequest>? addons)
    {
        if (addons is null || addons.Count == 0)
            return string.Empty;

        return string.Join(
            "|",
            addons
                .GroupBy(addon => addon.MenuAddonId)
                .Select(group => new
                {
                    MenuAddonId = group.Key,
                    Quantity = group.Sum(addon => addon.Quantity)
                })
                .OrderBy(addon => addon.MenuAddonId)
                .Select(addon =>
                    $"{addon.MenuAddonId:N}:{addon.Quantity.ToString("0.###", CultureInfo.InvariantCulture)}"));
    }

    private static string GenerateInvoiceNumber() =>
        $"INV-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
