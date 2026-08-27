using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappers;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IInvoiceRepository invoiceRepository,
    IMenuItemRepository menuItemRepository,
    IModifierGroupRepository modifierGroupRepository,
    IUserRepository userRepository,
    IUserAddressRepository userAddressRepository,
    ICompanyInfoRepository companyInfoRepository,
    IOrderNumberGenerator orderNumberGenerator,
    IInvoiceNumberGenerator invoiceNumberGenerator,
    IDomainEventDispatcher domainEventDispatcher,
    ILogger<OrderService> logger)
{
    public async Task<OrderResponse> RegisterOrderAsync(RegisterOrderRequest request, Guid userId)
    {
        if (request.Items is null || request.Items.Count == 0)
            throw new DomainException("Order must contain at least one item.");

        var orderNumber = await orderNumberGenerator.NextAsync();
        var order = Order.CreatePosDraft(orderNumber, userId, request.TableNumber);

        foreach (var requestItem in request.Items)
        {
            var menuItem = await menuItemRepository.GetByIdAsync(requestItem.MenuItemId)
                ?? throw new NotFoundException($"Menu item with id '{requestItem.MenuItemId}' was not found.");

            if (!menuItem.IsAvailable)
                throw new DomainException($"Menu item '{menuItem.Name}' is not available.");

            var item = order.AddItem(menuItem.Id, menuItem.Name, Money.Create(menuItem.Price), requestItem.Quantity);

            if (requestItem.Addons is not null && requestItem.Addons.Count > 0)
            {
                var modifierIds = requestItem.Addons.Select(a => a.ModifierId).Distinct().ToList();
                var modifiers = await modifierGroupRepository.GetModifiersByIdsAsync(modifierIds);
                if (modifiers.Count != modifierIds.Count)
                    throw new NotFoundException("One or more modifiers were not found.");

                var modifierGroups = await modifierGroupRepository.GetByMenuItemAsync(menuItem.Id);
                var applicableModifierIds = modifierGroups.SelectMany(g => g.Modifiers).Select(m => m.Id).ToHashSet();

                foreach (var addonRequest in requestItem.Addons)
                {
                    var modifier = modifiers.FirstOrDefault(m => m.Id == addonRequest.ModifierId)
                        ?? throw new NotFoundException($"Modifier with id '{addonRequest.ModifierId}' was not found.");

                    if (!modifier.IsAvailable)
                        throw new DomainException($"Modifier '{modifier.Name}' is not available.");

                    if (!applicableModifierIds.Contains(modifier.Id))
                        throw new DomainException($"Modifier '{modifier.Name}' is not applicable to menu item '{menuItem.Name}'.");

                    item.AddAddon(OrderItemAddon.Create(modifier.Id, modifier.Name, addonRequest.Quantity * requestItem.Quantity, modifier.Price));
                }
            }
        }

        var invoiceNumber = await invoiceNumberGenerator.NextAsync();
        var invoice = Invoice.CreateDraft(order, invoiceNumber, userId);

        await orderRepository.AddAsync(order);
        await invoiceRepository.AddAsync(invoice);
        await orderRepository.SaveChangesAsync();

        await PopulateInvoiceItemsFromOrderAsync(order, invoice);

        order.Register();

        var companyInfo = await GetCompanyInfoAsync();
        var taxRate = companyInfo is { TaxEnabled: true } ? companyInfo.TaxRate : 0m;
        invoice.RecalculateFromOrder(invoice.Discount, taxRate);
        invoice.MarkPendingPayment();

        await orderRepository.SaveChangesAsync();

        var events = order.DomainEvents.Concat(invoice.DomainEvents).ToList();
        order.ClearDomainEvents();
        invoice.ClearDomainEvents();
        await domainEventDispatcher.DispatchAsync(events);

        logger.LogInformation("Order {OrderId} registered with number {OrderNumber}. Invoice {InvoiceNumber} moved to PendingPayment.", order.Id, order.OrderNumber, invoice.InvoiceNumber);
        return order.ToResponse(invoice);
    }

    public async Task<OrderResponse> SetTableNumberAsync(Guid orderId, string? tableNumber)
    {
        var order = await GetOrderAsync(orderId);

        order.SetTableNumber(tableNumber);

        await orderRepository.SaveChangesAsync();

        var invoice = await GetDraftInvoiceAsync(order.Id);
        return order.ToResponse(invoice);
    }

    public async Task CancelAsync(Guid orderId)
    {
        var order = await GetOrderAsync(orderId);

        order.Cancel();

        var invoice = await GetDraftInvoiceAsync(order.Id);
        if (invoice is not null)
        {
            var userId = order.CreatedByUserId ?? Guid.Empty;
            invoice.Cancel(userId);
        }

        await orderRepository.SaveChangesAsync();
        logger.LogInformation("Order {OrderId} was cancelled.", order.Id);
    }

    public async Task<OrderResponse> SubmitExternalOrderAsync(Guid userId, SubmitExternalOrderRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
            throw new DomainException("External order must contain at least one item.");

        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with id '{userId}' was not found.");

        var address = await userAddressRepository.GetByIdAsync(request.AddressId)
            ?? throw new NotFoundException($"Address with id '{request.AddressId}' was not found.");

        if (address.UserId != userId)
            throw new NotFoundException($"Address with id '{request.AddressId}' was not found.");

        var items = new List<OrderItem>();
        foreach (var requestItem in request.Items)
        {
            var menuItem = await menuItemRepository.GetByIdAsync(requestItem.MenuItemId)
                ?? throw new NotFoundException($"Menu item with id '{requestItem.MenuItemId}' was not found.");

            if (!menuItem.IsAvailable)
                throw new DomainException($"Menu item '{menuItem.Name}' is not available.");

            items.Add(OrderItem.Create(menuItem.Id, menuItem.Name, Money.Create(menuItem.Price), requestItem.Quantity));
        }

        var orderNumber = await orderNumberGenerator.NextAsync();
        var order = Order.CreateWebOrder(orderNumber, userId, $"{user.FirstName} {user.LastName}", address, items);

        var invoiceNumber = await invoiceNumberGenerator.NextAsync();
        var invoice = Invoice.CreateDraft(order, invoiceNumber, userId);

        await orderRepository.AddAsync(order);
        await invoiceRepository.AddAsync(invoice);
        await orderRepository.SaveChangesAsync();

        logger.LogInformation("External order {OrderId} submitted with number {OrderNumber}.", order.Id, order.OrderNumber);
        return order.ToResponse(invoice);
    }

    public async Task<OrderResponse> ApproveAsync(Guid orderId, Guid reviewerId)
    {
        var order = await GetOrderAsync(orderId);

        order.Approve(reviewerId);

        var invoice = await GetDraftInvoiceAsync(order.Id)
            ?? throw new NotFoundException("Draft invoice not found for this order.");

        await ApplyCurrentPriceSnapshotsAsync(order);
        await PopulateInvoiceItemsFromOrderAsync(order, invoice);

        var companyInfo = await GetCompanyInfoAsync();
        var taxRate = companyInfo is { TaxEnabled: true } ? companyInfo.TaxRate : 0m;
        var discount = invoice.Discount;

        invoice.RecalculateFromOrder(discount, taxRate);
        invoice.MarkPendingPayment();

        await orderRepository.SaveChangesAsync();

        var events = order.DomainEvents.Concat(invoice.DomainEvents).ToList();
        order.ClearDomainEvents();
        invoice.ClearDomainEvents();
        await domainEventDispatcher.DispatchAsync(events);

        logger.LogInformation("Order {OrderId} was approved. Invoice moved to PendingPayment.", order.Id);
        return order.ToResponse(invoice);
    }

    public async Task RejectAsync(Guid orderId, Guid reviewerId, string reason)
    {
        var order = await GetOrderAsync(orderId);

        order.Reject(reviewerId, reason);

        await orderRepository.SaveChangesAsync();
        logger.LogInformation("Order {OrderId} was rejected.", order.Id);
    }

    public async Task<OrderResponse> GetByIdAsync(Guid orderId)
    {
        var order = await GetOrderAsync(orderId);
        var invoice = await GetDraftInvoiceAsync(order.Id);
        return order.ToResponse(invoice);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetQueueAsync(OrderStatus? status)
    {
        var orders = await orderRepository.GetByStatusAsync(status ?? OrderStatus.Registered);
        var responses = new List<OrderResponse>();

        foreach (var order in orders)
        {
            var invoice = await GetDraftInvoiceAsync(order.Id);
            responses.Add(order.ToResponse(invoice));
        }

        return responses;
    }

    private async Task<Order> GetOrderAsync(Guid orderId) =>
        await orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id '{orderId}' was not found.");

    private async Task<Invoice?> GetDraftInvoiceAsync(Guid orderId) =>
        await invoiceRepository.GetByOrderIdAsync(orderId);

    private async Task PopulateInvoiceItemsFromOrderAsync(Order order, Invoice invoice)
    {
        var existingItems = invoice.Items.ToList();
        foreach (var existingItem in existingItems)
        {
            invoice.RemoveItem(existingItem);
        }

        foreach (var orderItem in order.Items)
        {
            var invoiceItem = InvoiceItem.Create(
                orderItem.MenuItemId,
                orderItem.ProductName,
                orderItem.Quantity,
                orderItem.UnitPrice.Amount);

            foreach (var addon in orderItem.Addons)
            {
                invoiceItem.AddAddon(InvoiceItemAddon.Create(
                    addon.ModifierId,
                    addon.AddonName,
                    addon.Quantity,
                    addon.UnitPrice));
            }

            invoice.AddItem(invoiceItem);
        }
    }

    private async Task ApplyCurrentPriceSnapshotsAsync(Order order)
    {
        var prices = new Dictionary<Guid, Money>();

        foreach (var item in order.Items)
        {
            if (prices.ContainsKey(item.MenuItemId))
                continue;

            var menuItem = await menuItemRepository.GetByIdAsync(item.MenuItemId);
            if (menuItem is not null)
                prices[item.MenuItemId] = Money.Create(menuItem.Price);
        }

        order.ApplyPriceSnapshots(prices);
    }

    private async Task<Domain.Entities.CompanyInfo?> GetCompanyInfoAsync() =>
        await companyInfoRepository.GetAsync();
}
