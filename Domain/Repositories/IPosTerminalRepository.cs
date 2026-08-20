using Domain.Entities;

namespace Domain.Repositories;

public interface IPosTerminalRepository
{
    Task<PosTerminalDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PosTerminalDefinition?> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PosTerminalDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(PosTerminalDefinition terminal, CancellationToken cancellationToken = default);
    void Remove(PosTerminalDefinition terminal);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
