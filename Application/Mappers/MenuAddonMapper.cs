using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class MenuAddonMapper
{
    public static MenuAddonResponse ToResponse(
        this MenuAddon addon,
        IReadOnlyList<Guid> applicableMenuItemIds) =>
        new(
            addon.Id,
            addon.Name,
            addon.Description,
            addon.Price,
            addon.IsAvailable,
            addon.DisplayOrder,
            applicableMenuItemIds);

    public static MenuAddonRecipeResponse ToResponse(
        this MenuAddonRecipe recipe,
        IReadOnlyDictionary<Guid, Ingredient> ingredients,
        IReadOnlyDictionary<Guid, MeasurementUnit> units) =>
        new(
            recipe.Id,
            recipe.MenuAddonId,
            recipe.Components
                .Select(component =>
                {
                    var ingredient = ingredients[component.IngredientId];
                    var unit = units[ingredient.MeasurementUnitId];
                    return new MenuAddonRecipeComponentResponse(
                        component.Id,
                        component.IngredientId,
                        ingredient.Name,
                        unit.ToResponse(),
                        component.Quantity);
                })
                .OrderBy(component => component.IngredientName)
                .ToList());
}
