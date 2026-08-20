using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class InventoryRepository(AppDbContext dbContext) : IInventoryRepository
{
    public async Task<MeasurementUnit?> GetUnitByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await dbContext.MeasurementUnits
            .FirstOrDefaultAsync(unit => unit.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MeasurementUnit>> GetUnitsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        await dbContext.MeasurementUnits
            .AsNoTracking()
            .Where(unit => includeInactive || unit.IsActive)
            .OrderBy(unit => unit.Name)
            .ToListAsync(cancellationToken);

    public async Task<bool> UnitNameOrSymbolExistsAsync(
        string name,
        string symbol,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default) =>
        await dbContext.MeasurementUnits.AnyAsync(
            unit =>
                (!excludingId.HasValue || unit.Id != excludingId.Value) &&
                (EF.Functions.ILike(unit.Name, name.Trim()) ||
                 EF.Functions.ILike(unit.Symbol, symbol.Trim())),
            cancellationToken);

    public async Task AddUnitAsync(
        MeasurementUnit unit,
        CancellationToken cancellationToken = default) =>
        await dbContext.MeasurementUnits.AddAsync(unit, cancellationToken);

    public async Task<Ingredient?> GetIngredientByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await dbContext.Ingredients
            .FirstOrDefaultAsync(ingredient => ingredient.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Ingredient>> GetIngredientsAsync(
        bool includeInactive = false,
        bool lowStockOnly = false,
        CancellationToken cancellationToken = default) =>
        await dbContext.Ingredients
            .AsNoTracking()
            .Where(ingredient =>
                (includeInactive || ingredient.IsActive) &&
                (!lowStockOnly || ingredient.CurrentStock < ingredient.MinimumStock))
            .OrderBy(ingredient => ingredient.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Ingredient>> GetIngredientsByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default) =>
        await dbContext.Ingredients
            .Where(ingredient => ids.Contains(ingredient.Id))
            .ToListAsync(cancellationToken);

    public async Task<bool> IngredientNameExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default) =>
        await dbContext.Ingredients.AnyAsync(
            ingredient =>
                (!excludingId.HasValue || ingredient.Id != excludingId.Value) &&
                EF.Functions.ILike(ingredient.Name, name.Trim()),
            cancellationToken);

    public async Task AddIngredientAsync(
        Ingredient ingredient,
        CancellationToken cancellationToken = default) =>
        await dbContext.Ingredients.AddAsync(ingredient, cancellationToken);

    public async Task AddLedgerEntryAsync(
        StockLedgerEntry entry,
        CancellationToken cancellationToken = default) =>
        await dbContext.StockLedgerEntries.AddAsync(entry, cancellationToken);

    public async Task<IReadOnlyList<StockLedgerEntry>> GetLedgerAsync(
        Guid ingredientId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default) =>
        await dbContext.StockLedgerEntries
            .AsNoTracking()
            .Where(entry =>
                entry.IngredientId == ingredientId &&
                (!fromUtc.HasValue || entry.OccurredAtUtc >= fromUtc.Value) &&
                (!toUtc.HasValue || entry.OccurredAtUtc < toUtc.Value))
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<MenuItemRecipe?> GetRecipeByMenuItemIdAsync(
        Guid menuItemId,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuItemRecipes
            .Include(recipe => recipe.Components)
            .FirstOrDefaultAsync(recipe => recipe.MenuItemId == menuItemId, cancellationToken);

    public async Task<IReadOnlyList<MenuItemRecipe>> GetRecipesByMenuItemIdsAsync(
        IReadOnlyCollection<Guid> menuItemIds,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuItemRecipes
            .Include(recipe => recipe.Components)
            .Where(recipe => menuItemIds.Contains(recipe.MenuItemId))
            .ToListAsync(cancellationToken);

    public async Task AddRecipeAsync(
        MenuItemRecipe recipe,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuItemRecipes.AddAsync(recipe, cancellationToken);

    public async Task<MenuAddonRecipe?> GetRecipeByMenuAddonIdAsync(
        Guid menuAddonId,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuAddonRecipes
            .Include(recipe => recipe.Components)
            .FirstOrDefaultAsync(recipe => recipe.MenuAddonId == menuAddonId, cancellationToken);

    public async Task<IReadOnlyList<MenuAddonRecipe>> GetRecipesByMenuAddonIdsAsync(
        IReadOnlyCollection<Guid> menuAddonIds,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuAddonRecipes
            .Include(recipe => recipe.Components)
            .Where(recipe => menuAddonIds.Contains(recipe.MenuAddonId))
            .ToListAsync(cancellationToken);

    public async Task AddMenuAddonRecipeAsync(
        MenuAddonRecipe recipe,
        CancellationToken cancellationToken = default) =>
        await dbContext.MenuAddonRecipes.AddAsync(recipe, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InventoryConcurrencyException(
                "Inventory changed during this operation. Refresh the data and try again.");
        }
    }
}
