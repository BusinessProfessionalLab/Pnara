using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ModifierGroupRepository(AppDbContext dbContext) : IModifierGroupRepository
{
    public async Task<ModifierGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.ModifierGroups
            .Include(mg => mg.Modifiers)
            .FirstOrDefaultAsync(mg => mg.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ModifierGroup>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.ModifierGroups
            .Include(mg => mg.Modifiers)
            .OrderBy(mg => mg.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ModifierGroup>> GetByMenuItemAsync(Guid menuItemId, CancellationToken cancellationToken = default) =>
        await dbContext.ModifierGroupMenuItems
            .Where(x => x.MenuItemId == menuItemId)
            .Join(dbContext.ModifierGroups,
                x => x.ModifierGroupId,
                mg => mg.Id,
                (x, mg) => mg)
            .Include(mg => mg.Modifiers)
            .OrderBy(mg => mg.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Modifier>> GetModifiersByIdsAsync(IReadOnlyList<Guid> modifierIds, CancellationToken cancellationToken = default) =>
        await dbContext.Modifiers
            .Where(m => modifierIds.Contains(m.Id))
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ModifierGroup modifierGroup, CancellationToken cancellationToken = default) =>
        await dbContext.ModifierGroups.AddAsync(modifierGroup, cancellationToken);

    public async Task AddModifierAsync(Modifier modifier, CancellationToken cancellationToken = default) =>
        await dbContext.Modifiers.AddAsync(modifier, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
