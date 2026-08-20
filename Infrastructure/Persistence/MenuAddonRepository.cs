using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class MenuAddonRepository(AppDbContext dbContext) : IMenuAddonRepository
{
    public async Task<MenuAddon?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuAddons.FirstOrDefaultAsync(
            addon => addon.Id == id,
            cancellationToken);

    public async Task<IReadOnlyList<MenuAddon>> GetAllAsync(
        bool includeUnavailable = false,
        Guid? menuItemId = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.MenuAddons
            .AsNoTracking()
            .Where(addon => includeUnavailable || addon.IsAvailable);

        if (menuItemId.HasValue)
        {
            query = query.Where(addon => dbContext.MenuAddonMenuItems.Any(
                applicability =>
                    applicability.MenuAddonId == addon.Id &&
                    applicability.MenuItemId == menuItemId.Value));
        }

        return await query
            .OrderBy(addon => addon.DisplayOrder)
            .ThenBy(addon => addon.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MenuAddon>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuAddons
            .Where(addon => ids.Contains(addon.Id))
            .ToListAsync(cancellationToken);

    public async Task<bool> NameExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuAddons.AnyAsync(
            addon =>
                (!excludingId.HasValue || addon.Id != excludingId.Value) &&
                EF.Functions.ILike(addon.Name, name.Trim()),
            cancellationToken);

    public async Task AddAsync(
        MenuAddon addon,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuAddons.AddAsync(addon, cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetApplicableMenuItemIdsAsync(
        Guid addonId,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuAddonMenuItems
            .AsNoTracking()
            .Where(applicability => applicability.MenuAddonId == addonId)
            .Select(applicability => applicability.MenuItemId)
            .ToListAsync(cancellationToken);

    public async Task<bool> IsApplicableToMenuItemAsync(
        Guid addonId,
        Guid menuItemId,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuAddonMenuItems.AnyAsync(
            applicability =>
                applicability.MenuAddonId == addonId &&
                applicability.MenuItemId == menuItemId,
            cancellationToken);

    public async Task AddApplicabilityAsync(
        MenuAddonMenuItem applicability,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuAddonMenuItems.AddAsync(applicability, cancellationToken);

    public async Task<MenuAddonMenuItem?> GetApplicabilityAsync(
        Guid addonId,
        Guid menuItemId,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuAddonMenuItems.FirstOrDefaultAsync(
            applicability =>
                applicability.MenuAddonId == addonId &&
                applicability.MenuItemId == menuItemId,
            cancellationToken);

    public void RemoveApplicability(MenuAddonMenuItem applicability) =>
        dbContext.MenuAddonMenuItems.Remove(applicability);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
