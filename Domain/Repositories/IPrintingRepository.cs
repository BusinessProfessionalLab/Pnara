using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories;

public interface IPrintingRepository
{
    Task<PrinterDefinition?> GetPrinterByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrinterDefinition>> GetPrintersAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<bool> PrinterNameExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    Task AddPrinterAsync(
        PrinterDefinition printer,
        CancellationToken cancellationToken = default);

    Task<ReceiptTemplate?> GetTemplateAsync(
        ReceiptType receiptType,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReceiptTemplate>> GetTemplatesAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task AddTemplateAsync(
        ReceiptTemplate template,
        CancellationToken cancellationToken = default);

    Task<ReceiptPrinterMapping?> GetMappingAsync(
        ReceiptType receiptType,
        CancellationToken cancellationToken = default);

    Task AddMappingAsync(
        ReceiptPrinterMapping mapping,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
