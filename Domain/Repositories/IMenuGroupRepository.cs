using Domain.Entities;

namespace Domain.Repositories;

public interface IMenuGroupRepository
{
    Task<MenuGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuGroup>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(MenuGroup group, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
