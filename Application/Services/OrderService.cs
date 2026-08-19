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
    IUserRepository userRepository,
    IUserAddressRepository userAddressRepository,
    ICompanyInfoRepository companyInfoRepository,
    IOrderNumberGenerator orderNumberGenerator,
    IInvoiceNumberGenerator invoiceNumberGenerator,
    IDomainEventDispatcher domainEventDispatcher,
    ILogger<OrderService> logger)
{
    public async Task<OrderResponse> CreatePosDraftAsync(Guid userId, string? tableNumber = null)
    {
        var orderNumber = await orderNumberGenerator.NextAsync();
        var order = Order.CreatePosDraft(orderNumber, userId, tableNumber);

        var invoiceNumber = await invoiceNumberGenerator.NextAsync();
        var invoice = Invoice.CreateDraft(order, invoiceNumber, userId);

        await orderRepository.AddAsync(order);
        await invoiceRepository.AddAsync(invoice);
        await orderRepository.SaveChangesAsync();

        logger.LogInformation("POS draft order {OrderId} created with number {OrderNumber}, table {TableNumber}.", order.Id, order.OrderNumber, tableNumber);
        return order.ToResponse(invoice);
    }

    public async Task<OrderResponse> AddItemAsync(Guid orderId, AddOrderItemRequest request)
    {
        var order = await GetOrderAsync(orderId);

        var menuItem = await menuItemRepository.GetByIdAsync(request.MenuItemId)
            ?? throw new NotFoundException($"Menu item with id '{request.MenuItemId}' was not found.");

        if (!menuItem.IsAvailable)
            throw new DomainException($"Menu item '{menuItem.Name}' is not available.");

        order.AddItem(menuItem.Id, menuItem.Name, Money.Create(menuItem.Price), request.Quantity);

        await RecalculateDraftInvoiceAsync(order);
        await orderRepository.SaveChangesAsync();

        var invoice = await GetDraftInvoiceAsync(order.Id);
        return order.ToResponse(invoice);
    }

    public async Task<OrderResponse> RemoveItemAsync(Guid orderId, Guid orderItemId)
    {
        var order = await GetOrderAsync(orderId);

        order.RemoveItem(orderItemId);

        await RecalculateDraftInvoiceAsync(order);
        await orderRepository.SaveChangesAsync();

        var invoice = await GetDraftInvoiceAsync(order.Id);
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
        var customerName = $"{user.FirstName} {user.LastName}".Trim();
        var order = Order.CreateWebOrder(orderNumber, userId, customerName, address, items);

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

    public async Task<OrderResponse> RegisterAsync(Guid orderId)
    {
        var order = await GetOrderAsync(orderId);

        order.Register();

        var invoice = await GetDraftInvoiceAsync(order.Id)
            ?? throw new NotFoundException("Draft invoice not found for this order.");

        await ApplyCurrentPriceSnapshotsAsync(order);

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

        logger.LogInformation("Order {OrderId} registered. Invoice {InvoiceNumber} moved to PendingPayment.", order.Id, invoice.InvoiceNumber);
        return order.ToResponse(invoice);
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

    private async Task RecalculateDraftInvoiceAsync(Order order)
    {
        var invoice = await GetDraftInvoiceAsync(order.Id);
        if (invoice is null || invoice.PaymentStatus != PaymentStatus.Draft)
            return;

        invoice.RecalculateFromOrder(invoice.Discount, invoice.TaxRate);
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
