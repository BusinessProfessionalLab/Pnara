using Application.Common;
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

public class InvoiceService(
    IOrderRepository orderRepository,
    IInvoiceRepository invoiceRepository,
    IMenuItemRepository menuItemRepository,
    ICompanyInfoRepository companyInfoRepository,
    IInvoiceNumberGenerator invoiceNumberGenerator,
    IDomainEventDispatcher domainEventDispatcher,
    ILogger<InvoiceService> logger)
{
    public async Task<InvoiceResponse> RegisterAsync(Guid orderId, Guid issuedByUserId)
    {
        var order = await orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id '{orderId}' was not found.");

        if (order.Status == OrderStatus.Draft)
            order.Register();

        if (order.Status != OrderStatus.Registered)
            throw new DomainException("Order must be registered to finalize the invoice.");

        await ApplyCurrentPriceSnapshotsAsync(order);

        var invoice = await invoiceRepository.GetByOrderIdAsync(orderId)
            ?? throw new NotFoundException($"Draft invoice not found for order '{orderId}'.");

        var companyInfo = await companyInfoRepository.GetAsync();
        var taxRate = companyInfo is { TaxEnabled: true } ? companyInfo.TaxRate : 0m;

        invoice.RecalculateFromOrder(invoice.Discount, taxRate);
        invoice.MarkPendingPayment();

        await invoiceRepository.SaveChangesAsync();

        var events = order.DomainEvents.Concat(invoice.DomainEvents).ToList();
        order.ClearDomainEvents();
        invoice.ClearDomainEvents();
        await domainEventDispatcher.DispatchAsync(events);

        logger.LogInformation("Order {OrderId} registered. Invoice {InvoiceNumber} moved to PendingPayment.", order.Id, invoice.InvoiceNumber);
        return invoice.ToResponse(order);
    }

    public async Task<InvoiceResponse> IssueInvoiceAsync(Guid orderId, IssueInvoiceRequest request, Guid issuedByUserId)
    {
        var order = await orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order with id '{orderId}' was not found.");

        if (order.Status == OrderStatus.Draft)
            order.Register();

        if (order.Status != OrderStatus.Registered)
            throw new DomainException("Invoice can only be issued for orders that are registered in the queue.");

        await ApplyCurrentPriceSnapshotsAsync(order);

        var invoice = await invoiceRepository.GetByOrderIdAsync(orderId)
            ?? throw new NotFoundException($"Draft invoice not found for order '{orderId}'.");

        var companyInfo = await companyInfoRepository.GetAsync();
        var taxRate = companyInfo is { TaxEnabled: true } ? companyInfo.TaxRate : 0m;

        invoice.RecalculateFromOrder(Money.Create(request.Discount), taxRate);
        invoice.MarkPendingPayment();

        await invoiceRepository.SaveChangesAsync();

        var events = order.DomainEvents.Concat(invoice.DomainEvents).ToList();
        order.ClearDomainEvents();
        invoice.ClearDomainEvents();
        await domainEventDispatcher.DispatchAsync(events);

        logger.LogInformation("Invoice {InvoiceNumber} issued for order {OrderId}.", invoice.InvoiceNumber, order.Id);
        return invoice.ToResponse(order);
    }

    public async Task<InvoiceResponse> PayInvoiceAsync(Guid invoiceId, Guid paidByUserId)
    {
        var (invoice, order) = await GetInvoiceWithOrderAsync(invoiceId);

        invoice.Pay(paidByUserId);
        order.MarkPaid();

        await invoiceRepository.SaveChangesAsync();

        var events = invoice.DomainEvents.ToList();
        invoice.ClearDomainEvents();
        await domainEventDispatcher.DispatchAsync(events);

        logger.LogInformation("Invoice {InvoiceNumber} was paid.", invoice.InvoiceNumber);
        return invoice.ToResponse(order);
    }

    public async Task<InvoiceResponse> CancelInvoiceAsync(Guid invoiceId, Guid cancelledByUserId)
    {
        var (invoice, order) = await GetInvoiceWithOrderAsync(invoiceId);

        invoice.Cancel(cancelledByUserId);
        order.CancelAfterInvoice();

        await invoiceRepository.SaveChangesAsync();

        logger.LogInformation("Invoice {InvoiceNumber} was cancelled.", invoice.InvoiceNumber);
        return invoice.ToResponse(order);
    }

    public async Task<InvoiceResponse> GetByIdAsync(Guid invoiceId)
    {
        var (invoice, order) = await GetInvoiceWithOrderAsync(invoiceId);
        return invoice.ToResponse(order);
    }

    public async Task<IReadOnlyList<InvoiceListItemResponse>> GetListAsync(string? fromJalali, string? toJalali, PaymentStatus? status)
    {
        DateTime? fromUtc = null;
        DateTime? toUtc = null;

        if (!string.IsNullOrWhiteSpace(fromJalali))
            fromUtc = ParseJalaliDay(fromJalali).FromUtc;

        if (!string.IsNullOrWhiteSpace(toJalali))
            toUtc = ParseJalaliDay(toJalali).ToUtc;

        var invoices = await invoiceRepository.GetListAsync(fromUtc, toUtc, status);
        return invoices.Select(invoice => invoice.ToListItem(invoice.Order)).ToList();
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

    private async Task<(Invoice Invoice, Order Order)> GetInvoiceWithOrderAsync(Guid invoiceId)
    {
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId)
            ?? throw new NotFoundException($"Invoice with id '{invoiceId}' was not found.");

        var order = invoice.Order
            ?? await orderRepository.GetByIdAsync(invoice.OrderId)
            ?? throw new NotFoundException($"Order with id '{invoice.OrderId}' was not found.");

        return (invoice, order);
    }

    private static (DateTime FromUtc, DateTime ToUtc) ParseJalaliDay(string value)
    {
        try
        {
            return PersianDateTime.JalaliDayToUtcRange(value);
        }
        catch (FormatException exception)
        {
            throw new DomainException(exception.Message);
        }
    }
}
