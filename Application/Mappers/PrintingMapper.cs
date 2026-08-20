using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class PrintingMapper
{
    public static PrinterResponse ToResponse(this PrinterDefinition printer) =>
        new(
            printer.Id,
            printer.Name,
            printer.ConnectionType,
            printer.Host,
            printer.Port,
            printer.PaperWidth,
            printer.IsActive);

    public static ReceiptTemplateResponse ToResponse(this ReceiptTemplate template) =>
        new(
            template.Id,
            template.ReceiptType,
            template.HeaderText,
            template.FooterText,
            template.ShowLogo,
            template.ShowPrices,
            template.ShowDiscount,
            template.ShowTax,
            template.ShowPaymentMethod,
            template.ShowChannel,
            template.FontSize,
            template.IsActive);
}
