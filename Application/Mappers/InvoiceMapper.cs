using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class InvoiceMapper
{
    public static InvoiceResponse ToResponse(this Invoice invoice) =>
        new(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.Channel,
            invoice.Status,
            invoice.PaymentMethod,
            invoice.Subtotal,
            invoice.DiscountAmount,
            invoice.TaxAmount,
            invoice.TotalAmount,
            invoice.IssuedAtUtc,
            invoice.FinalizedAtUtc,
            invoice.Items
                .Select(item => new InvoiceItemResponse(
                    item.Id,
                    item.MenuItemId,
                    item.ItemName,
                    item.Quantity,
                    item.UnitPrice,
                    item.LineTotal))
                .ToList());
}
