using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs;

public record CreatePrinterRequest(
    [Required, MaxLength(200)] string Name,
    PrinterConnectionType ConnectionType,
    [Required, MaxLength(200)] string Host,
    [Range(1, 65535)] int Port,
    int PaperWidth);

public record UpdatePrinterRequest(
    [Required, MaxLength(200)] string Name,
    PrinterConnectionType ConnectionType,
    [Required, MaxLength(200)] string Host,
    [Range(1, 65535)] int Port,
    int PaperWidth,
    bool IsActive);

public record UpsertReceiptTemplateRequest(
    [MaxLength(1000)] string? HeaderText,
    [MaxLength(1000)] string? FooterText,
    bool ShowLogo,
    bool ShowPrices,
    bool ShowDiscount,
    bool ShowTax,
    bool ShowPaymentMethod,
    bool ShowChannel,
    [Range(1, 3)] int FontSize,
    bool IsActive = true);

public record AssignReceiptPrinterRequest(Guid PrinterDefinitionId);
