using Domain.Entities;

namespace Domain.Repositories;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuItem>> GetByGroupAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task AddAsync(MenuItem item, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
