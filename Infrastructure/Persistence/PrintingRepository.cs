using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class PrintingRepository(AppDbContext dbContext) : IPrintingRepository
{
    public async Task<PrinterDefinition?> GetPrinterByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await dbContext.PrinterDefinitions.FirstOrDefaultAsync(
            printer => printer.Id == id,
            cancellationToken);

    public async Task<IReadOnlyList<PrinterDefinition>> GetPrintersAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        await dbContext.PrinterDefinitions
            .AsNoTracking()
            .Where(printer => includeInactive || printer.IsActive)
            .OrderBy(printer => printer.Name)
            .ToListAsync(cancellationToken);

    public async Task<bool> PrinterNameExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default) =>
        await dbContext.PrinterDefinitions.AnyAsync(
            printer =>
                (!excludingId.HasValue || printer.Id != excludingId.Value) &&
                EF.Functions.ILike(printer.Name, name.Trim()),
            cancellationToken);

    public async Task AddPrinterAsync(
        PrinterDefinition printer,
        CancellationToken cancellationToken = default) =>
        await dbContext.PrinterDefinitions.AddAsync(printer, cancellationToken);

    public async Task<ReceiptTemplate?> GetTemplateAsync(
        ReceiptType receiptType,
        bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        await dbContext.ReceiptTemplates
            .FirstOrDefaultAsync(
                template =>
                    template.ReceiptType == receiptType &&
                    (includeInactive || template.IsActive),
                cancellationToken);

    public async Task<IReadOnlyList<ReceiptTemplate>> GetTemplatesAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        await dbContext.ReceiptTemplates
            .AsNoTracking()
            .Where(template => includeInactive || template.IsActive)
            .OrderBy(template => template.ReceiptType)
            .ToListAsync(cancellationToken);

    public async Task AddTemplateAsync(
        ReceiptTemplate template,
        CancellationToken cancellationToken = default) =>
        await dbContext.ReceiptTemplates.AddAsync(template, cancellationToken);

    public async Task<ReceiptPrinterMapping?> GetMappingAsync(
        ReceiptType receiptType,
        CancellationToken cancellationToken = default) =>
        await dbContext.ReceiptPrinterMappings
            .Include(mapping => mapping.PrinterDefinition)
            .FirstOrDefaultAsync(
                mapping => mapping.ReceiptType == receiptType,
                cancellationToken);

    public async Task AddMappingAsync(
        ReceiptPrinterMapping mapping,
        CancellationToken cancellationToken = default) =>
        await dbContext.ReceiptPrinterMappings.AddAsync(mapping, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
