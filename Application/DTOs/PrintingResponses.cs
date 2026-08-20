using Domain.Enums;

namespace Application.DTOs;

public record PrinterResponse(
    Guid Id,
    string Name,
    PrinterConnectionType ConnectionType,
    string Host,
    int Port,
    int PaperWidth,
    bool IsActive);

public record ReceiptTemplateResponse(
    Guid Id,
    ReceiptType ReceiptType,
    string? HeaderText,
    string? FooterText,
    bool ShowLogo,
    bool ShowPrices,
    bool ShowDiscount,
    bool ShowTax,
    bool ShowPaymentMethod,
    bool ShowChannel,
    int FontSize,
    bool IsActive);

public record ReceiptPrinterMappingResponse(
    ReceiptType ReceiptType,
    Guid PrinterDefinitionId,
    string PrinterName);

public record PrintReceiptResponse(
    Guid InvoiceId,
    ReceiptType ReceiptType,
    bool Printed,
    bool Skipped,
    string Message,
    string? PrinterName,
    DateTime AttemptedAtUtc);
