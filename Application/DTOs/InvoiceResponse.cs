using Domain.Enums;

namespace Application.DTOs;

public record InvoiceResponse(
    Guid Id,
    string InvoiceNumber,
    SalesChannel Channel,
    InvoiceStatus Status,
    PaymentMethod? PaymentMethod,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    DateTime IssuedAtUtc,
    DateTime? FinalizedAtUtc,
    IReadOnlyList<InvoiceItemResponse> Items);

public record InvoiceItemResponse(
    Guid Id,
    Guid MenuItemId,
    string ItemName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);
