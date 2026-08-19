namespace Application.DTOs;

public record InvoiceResponse(
    Guid Id,
    long InvoiceNumber,
    Guid OrderId,
    long OrderNumber,
    string Channel,
    DateTime IssuedAtUtc,
    string IssuedAtJalali,
    decimal SubTotal,
    decimal Discount,
    decimal TaxRate,
    decimal Tax,
    decimal GrandTotal,
    string Currency,
    string PaymentStatus,
    DateTime? PaidAtUtc,
    DateTime? CancelledAtUtc,
    string? CustomerName,
    string? DeliveryAddressLine,
    string? DeliveryCity,
    string? DeliveryPhoneNumber,
    IReadOnlyList<OrderItemResponse> Items);

public record InvoiceListItemResponse(
    Guid Id,
    long InvoiceNumber,
    Guid OrderId,
    long OrderNumber,
    string Channel,
    DateTime IssuedAtUtc,
    string IssuedAtJalali,
    decimal GrandTotal,
    string Currency,
    string PaymentStatus,
    string? TableNumber,
    string? CustomerName);
