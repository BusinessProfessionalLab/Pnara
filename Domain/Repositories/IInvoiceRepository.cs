using Domain.Entities;
using Domain.Enums;

namespace Domain.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invoice>> GetListAsync(DateTime? fromUtc = null, DateTime? toUtc = null, PaymentStatus? status = null, CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Invoice>> GetFinalizedForReportAsync(
        DateTime fromUtc,
        DateTime toUtc,
        SalesChannel? channel = null,
        PaymentMethod? paymentMethod = null,
        CancellationToken cancellationToken = default);
}
