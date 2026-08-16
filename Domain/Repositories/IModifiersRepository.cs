using Domain.Entities;

namespace Domain.Repositories
{
    public interface IModifiersRepository
    {
        Task<Modifiers?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Modifiers>> GetByMenuItemIdAsync(Guid menuItemId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Modifiers modifier, CancellationToken cancellationToken = default);
        void Update(Modifiers modifier);
        void Remove(Modifiers modifier);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}
