using Application.DTOs;
using Application.Exceptions;
using Application.Mappers;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;

namespace Application.Services;

public class InvoiceService(
    IInvoiceRepository invoiceRepository,
    IMenuItemRepository menuItemRepository,
    IInventoryRepository inventoryRepository)
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

        foreach (var itemRequest in request.Items.GroupBy(item => item.MenuItemId))
        {
            var menuItem = await menuItemRepository.GetByIdAsync(itemRequest.Key, cancellationToken)
                ?? throw new NotFoundException($"Menu item with id '{itemRequest.Key}' was not found.");

            if (!menuItem.IsAvailable)
                throw new DomainException($"Menu item '{menuItem.Name}' is not available.");

            var quantity = itemRequest.Sum(item => item.Quantity);
            invoice.AddItem(InvoiceItem.Create(
                menuItem.Id,
                menuItem.Name,
                quantity,
                menuItem.Price));
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

        return invoice.ToResponse();
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

        var requiredQuantities = invoice.Items
            .SelectMany(item => recipeByMenuItemId[item.MenuItemId].Components.Select(
                component => new
                {
                    component.IngredientId,
                    Quantity = component.Quantity * item.Quantity
                }))
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

    private static string GenerateInvoiceNumber() =>
        $"INV-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
