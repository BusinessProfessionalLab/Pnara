using Domain.Entities;

namespace Domain.Repositories;

public interface IModifierGroupRepository
{
    Task<ModifierGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ModifierGroup>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ModifierGroup>> GetByMenuItemAsync(Guid menuItemId, CancellationToken cancellationToken = default);
    Task AddAsync(ModifierGroup modifierGroup, CancellationToken cancellationToken = default);
    Task AddModifierAsync(Modifier modifier, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
