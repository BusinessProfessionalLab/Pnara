using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class InvoiceRepository(AppDbContext dbContext) : IInvoiceRepository
{
    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Invoices
            .Include(invoice => invoice.Items)
            .FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default) =>
        await dbContext.Invoices.AddAsync(invoice, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<Invoice>> GetFinalizedForReportAsync(
        DateTime fromUtc,
        DateTime toUtc,
        SalesChannel? channel = null,
        PaymentMethod? paymentMethod = null,
        CancellationToken cancellationToken = default) =>
        await dbContext.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .Where(invoice =>
                invoice.Status == InvoiceStatus.Finalized &&
                invoice.FinalizedAtUtc >= fromUtc &&
                invoice.FinalizedAtUtc < toUtc &&
                (!channel.HasValue || invoice.Channel == channel.Value) &&
                (!paymentMethod.HasValue || invoice.PaymentMethod == paymentMethod.Value))
            .OrderBy(invoice => invoice.IssuedAtUtc)
            .ToListAsync(cancellationToken);
}
