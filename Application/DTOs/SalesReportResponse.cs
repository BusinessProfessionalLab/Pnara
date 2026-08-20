namespace Application.DTOs;

public record SalesReportResponse(
    DateTime FromUtc,
    DateTime ToUtc,
    int InvoiceCount,
    decimal GrossSales,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal NetSales,
    IReadOnlyList<SalesChannelSummary> ByChannel,
    IReadOnlyList<PaymentMethodSummary> ByPaymentMethod,
    IReadOnlyList<TopSellingItem> TopItems);

public record SalesChannelSummary(
    string Channel,
    int InvoiceCount,
    decimal NetSales);

public record PaymentMethodSummary(
    string PaymentMethod,
    int InvoiceCount,
    decimal NetSales);

public record TopSellingItem(
    Guid MenuItemId,
    string ItemName,
    decimal Quantity,
    decimal Sales);
