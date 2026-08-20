using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class InventoryMapper
{
    public static MeasurementUnitResponse ToResponse(this MeasurementUnit unit) =>
        new(unit.Id, unit.Name, unit.Symbol, unit.IsActive);

    public static IngredientResponse ToResponse(
        this Ingredient ingredient,
        MeasurementUnit unit) =>
        new(
            ingredient.Id,
            ingredient.Name,
            unit.ToResponse(),
            ingredient.CurrentStock,
            ingredient.MinimumStock,
            ingredient.IsLowStock,
            ingredient.IsActive);

    public static StockLedgerEntryResponse ToResponse(this StockLedgerEntry entry) =>
        new(
            entry.Id,
            entry.IngredientId,
            entry.MovementType,
            entry.QuantityChange,
            entry.BalanceAfter,
            entry.InvoiceId,
            entry.Note,
            entry.OccurredAtUtc);

    public static MenuItemRecipeResponse ToResponse(
        this MenuItemRecipe recipe,
        IReadOnlyDictionary<Guid, Ingredient> ingredients,
        IReadOnlyDictionary<Guid, MeasurementUnit> units) =>
        new(
            recipe.Id,
            recipe.MenuItemId,
            recipe.Components
                .Select(component =>
                {
                    var ingredient = ingredients[component.IngredientId];
                    var unit = units[ingredient.MeasurementUnitId];
                    return new RecipeComponentResponse(
                        component.Id,
                        component.IngredientId,
                        ingredient.Name,
                        unit.ToResponse(),
                        component.Quantity);
                })
                .OrderBy(component => component.IngredientName)
                .ToList());
}
