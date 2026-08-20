using Domain.Entities;

namespace Domain.Repositories;

public interface IInventoryRepository
{
    Task<MeasurementUnit?> GetUnitByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MeasurementUnit>> GetUnitsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<bool> UnitNameOrSymbolExistsAsync(
        string name,
        string symbol,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    Task AddUnitAsync(
        MeasurementUnit unit,
        CancellationToken cancellationToken = default);

    Task<Ingredient?> GetIngredientByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ingredient>> GetIngredientsAsync(
        bool includeInactive = false,
        bool lowStockOnly = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ingredient>> GetIngredientsByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<bool> IngredientNameExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    Task AddIngredientAsync(
        Ingredient ingredient,
        CancellationToken cancellationToken = default);

    Task AddLedgerEntryAsync(
        StockLedgerEntry entry,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockLedgerEntry>> GetLedgerAsync(
        Guid ingredientId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default);

    Task<MenuItemRecipe?> GetRecipeByMenuItemIdAsync(
        Guid menuItemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MenuItemRecipe>> GetRecipesByMenuItemIdsAsync(
        IReadOnlyCollection<Guid> menuItemIds,
        CancellationToken cancellationToken = default);

    Task AddRecipeAsync(
        MenuItemRecipe recipe,
        CancellationToken cancellationToken = default);

    Task<MenuAddonRecipe?> GetRecipeByMenuAddonIdAsync(
        Guid menuAddonId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MenuAddonRecipe>> GetRecipesByMenuAddonIdsAsync(
        IReadOnlyCollection<Guid> menuAddonIds,
        CancellationToken cancellationToken = default);

    Task AddMenuAddonRecipeAsync(
        MenuAddonRecipe recipe,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
