using Application.Common;
using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class InvoiceMapper
{
    public static InvoiceResponse ToResponse(this Invoice invoice, Order order) =>
        new(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.OrderId,
            order.OrderNumber,
            order.Channel.ToString(),
            invoice.IssuedAtUtc,
            PersianDateTime.ToJalaliString(invoice.IssuedAtUtc),
            invoice.SubTotal.Amount,
            invoice.Discount.Amount,
            invoice.TaxRate,
            invoice.Tax.Amount,
            invoice.GrandTotal.Amount,
            invoice.GrandTotal.Currency,
            invoice.PaymentStatus.ToString(),
            invoice.PaidAtUtc,
            invoice.CancelledAtUtc,
            order.CustomerName,
            order.DeliveryAddressLine,
            order.DeliveryCity,
            order.DeliveryPhoneNumber,
            order.Items.Select(item => item.ToResponse()).ToList());

    public static InvoiceListItemResponse ToListItem(this Invoice invoice, Order order) =>
        new(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.OrderId,
            order.OrderNumber,
            order.Channel.ToString(),
            invoice.IssuedAtUtc,
            PersianDateTime.ToJalaliString(invoice.IssuedAtUtc),
            invoice.GrandTotal.Amount,
            invoice.GrandTotal.Currency,
            invoice.PaymentStatus.ToString(),
            order.TableNumber,
            order.CustomerName);
}
