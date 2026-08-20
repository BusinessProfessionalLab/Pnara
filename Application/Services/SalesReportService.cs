using Application.DTOs;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;

namespace Application.Services;

public class SalesReportService(IInvoiceRepository invoiceRepository)
{
    public async Task<SalesReportResponse> GetSalesAsync(
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        SalesChannel? channel = null,
        PaymentMethod? paymentMethod = null,
        int top = 10,
        CancellationToken cancellationToken = default)
    {
        var (from, to) = NormalizeRange(fromUtc, toUtc);
        if (to <= from)
            throw new DomainException("Report end date must be after the start date.");

        if (top is < 1 or > 100)
            throw new DomainException("Top item count must be between 1 and 100.");

        var invoices = await invoiceRepository.GetFinalizedForReportAsync(
            from,
            to,
            channel,
            paymentMethod,
            cancellationToken);

        var byChannel = invoices
            .GroupBy(invoice => invoice.Channel)
            .OrderBy(group => group.Key)
            .Select(group => new SalesChannelSummary(
                group.Key.ToString(),
                group.Count(),
                group.Sum(invoice => invoice.TotalAmount)))
            .ToList();

        var byPaymentMethod = invoices
            .Where(invoice => invoice.PaymentMethod.HasValue)
            .GroupBy(invoice => invoice.PaymentMethod!.Value)
            .OrderBy(group => group.Key)
            .Select(group => new PaymentMethodSummary(
                group.Key.ToString(),
                group.Count(),
                group.Sum(invoice => invoice.TotalAmount)))
            .ToList();

        var topItems = invoices
            .SelectMany(invoice => invoice.Items)
            .GroupBy(item => item.MenuItemId)
            .Select(group => new TopSellingItem(
                group.Key,
                group.Select(item => item.ItemName).First(),
                group.Sum(item => item.Quantity),
                group.Sum(item => item.LineTotal)))
            .OrderByDescending(item => item.Sales)
            .ThenByDescending(item => item.Quantity)
            .ThenBy(item => item.ItemName)
            .Take(top)
            .ToList();

        return new SalesReportResponse(
            from,
            to,
            invoices.Count,
            invoices.Sum(invoice => invoice.Subtotal),
            invoices.Sum(invoice => invoice.DiscountAmount),
            invoices.Sum(invoice => invoice.TaxAmount),
            invoices.Sum(invoice => invoice.TotalAmount),
            byChannel,
            byPaymentMethod,
            topItems);
    }

    private static (DateTime FromUtc, DateTime ToUtc) NormalizeRange(DateTime? fromUtc, DateTime? toUtc)
    {
        var defaultFrom = DateTime.UtcNow.Date;
        var from = fromUtc.HasValue ? EnsureUtc(fromUtc.Value) : defaultFrom;
        var to = toUtc.HasValue ? EnsureUtc(toUtc.Value) : from.AddDays(1);
        return (from, to);
    }

    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
