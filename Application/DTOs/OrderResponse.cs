namespace Application.DTOs;

public record OrderItemResponse(
    Guid Id,
    Guid MenuItemId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal);

public record OrderResponse(
    Guid Id,
    long OrderNumber,
    string Channel,
    string Status,
    DateTime CreatedAtUtc,
    string CreatedAtJalali,
    DateTime? RegisteredAtUtc,
    string? TableNumber,
    string? CustomerName,
    string? CustomerPhoneNumber,
    string? DeliveryAddressTitle,
    string? DeliveryAddressLine,
    string? DeliveryCity,
    string? DeliveryPostalCode,
    string? DeliveryPhoneNumber,
    string? RejectionReason,
    decimal? SubTotal,
    IReadOnlyList<OrderItemResponse> Items,
    InvoiceResponse? Invoice);
