using Application.DTOs;
using Application.Exceptions;
using Application.Mappers;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;

namespace Application.Services;

public class InventoryService(
    IInventoryRepository inventoryRepository,
    IMenuItemRepository menuItemRepository,
    IMenuAddonRepository menuAddonRepository)
{
    public async Task<MeasurementUnitResponse> CreateUnitAsync(
        CreateMeasurementUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await inventoryRepository.UnitNameOrSymbolExistsAsync(
                request.Name,
                request.Symbol,
                cancellationToken: cancellationToken))
        {
            throw new DomainException("A measurement unit with the same name or symbol already exists.");
        }

        var unit = MeasurementUnit.Create(request.Name, request.Symbol);
        await inventoryRepository.AddUnitAsync(unit, cancellationToken);
        await inventoryRepository.SaveChangesAsync(cancellationToken);

        return unit.ToResponse();
    }

    public async Task<MeasurementUnitResponse> UpdateUnitAsync(
        Guid id,
        UpdateMeasurementUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var unit = await inventoryRepository.GetUnitByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Measurement unit with id '{id}' was not found.");

        if (await inventoryRepository.UnitNameOrSymbolExistsAsync(
                request.Name,
                request.Symbol,
                id,
                cancellationToken))
        {
            throw new DomainException("A measurement unit with the same name or symbol already exists.");
        }

        unit.Update(request.Name, request.Symbol, request.IsActive);
        await inventoryRepository.SaveChangesAsync(cancellationToken);

        return unit.ToResponse();
    }

    public async Task<IReadOnlyList<MeasurementUnitResponse>> GetUnitsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var units = await inventoryRepository.GetUnitsAsync(includeInactive, cancellationToken);
        return units.Select(unit => unit.ToResponse()).ToList();
    }

    public async Task<IngredientResponse> CreateIngredientAsync(
        CreateIngredientRequest request,
        CancellationToken cancellationToken = default)
    {
        var unit = await GetActiveUnitAsync(request.MeasurementUnitId, cancellationToken);

        if (await inventoryRepository.IngredientNameExistsAsync(
                request.Name,
                cancellationToken: cancellationToken))
        {
            throw new DomainException("An ingredient with the same name already exists.");
        }

        var ingredient = Ingredient.Create(
            request.MeasurementUnitId,
            request.Name,
            request.OpeningStock,
            request.MinimumStock);

        await inventoryRepository.AddIngredientAsync(ingredient, cancellationToken);

        if (request.OpeningStock > 0)
        {
            await inventoryRepository.AddLedgerEntryAsync(
                StockLedgerEntry.Create(
                    ingredient.Id,
                    StockMovementType.OpeningBalance,
                    request.OpeningStock,
                    ingredient.CurrentStock,
                    note: "Opening balance"),
                cancellationToken);
        }

        await inventoryRepository.SaveChangesAsync(cancellationToken);
        return ingredient.ToResponse(unit);
    }

    public async Task<IngredientResponse> UpdateIngredientAsync(
        Guid id,
        UpdateIngredientRequest request,
        CancellationToken cancellationToken = default)
    {
        var ingredient = await inventoryRepository.GetIngredientByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Ingredient with id '{id}' was not found.");

        var unit = await GetActiveUnitAsync(request.MeasurementUnitId, cancellationToken);

        if (await inventoryRepository.IngredientNameExistsAsync(
                request.Name,
                id,
                cancellationToken))
        {
            throw new DomainException("An ingredient with the same name already exists.");
        }

        ingredient.Update(
            request.MeasurementUnitId,
            request.Name,
            request.MinimumStock,
            request.IsActive);

        await inventoryRepository.SaveChangesAsync(cancellationToken);
        return ingredient.ToResponse(unit);
    }

    public async Task<IReadOnlyList<IngredientResponse>> GetIngredientsAsync(
        bool includeInactive = false,
        bool lowStockOnly = false,
        CancellationToken cancellationToken = default)
    {
        var ingredients = await inventoryRepository.GetIngredientsAsync(
            includeInactive,
            lowStockOnly,
            cancellationToken);
        var units = await GetUnitMapAsync(cancellationToken);

        return ingredients
            .Select(ingredient => ingredient.ToResponse(units[ingredient.MeasurementUnitId]))
            .ToList();
    }

    public async Task<IngredientResponse> AdjustStockAsync(
        Guid id,
        AdjustStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var ingredient = await inventoryRepository.GetIngredientByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Ingredient with id '{id}' was not found.");

        var balanceAfter = ingredient.AdjustStock(request.QuantityChange);
        await inventoryRepository.AddLedgerEntryAsync(
            StockLedgerEntry.Create(
                ingredient.Id,
                StockMovementType.ManualAdjustment,
                request.QuantityChange,
                balanceAfter,
                note: request.Note),
            cancellationToken);

        await inventoryRepository.SaveChangesAsync(cancellationToken);

        var unit = await inventoryRepository.GetUnitByIdAsync(
            ingredient.MeasurementUnitId,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Measurement unit with id '{ingredient.MeasurementUnitId}' was not found.");

        return ingredient.ToResponse(unit);
    }

    public async Task<IReadOnlyList<StockLedgerEntryResponse>> GetLedgerAsync(
        Guid ingredientId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        _ = await inventoryRepository.GetIngredientByIdAsync(ingredientId, cancellationToken)
            ?? throw new NotFoundException($"Ingredient with id '{ingredientId}' was not found.");

        DateTime? from = fromUtc.HasValue ? EnsureUtc(fromUtc.Value) : null;
        DateTime? to = toUtc.HasValue ? EnsureUtc(toUtc.Value) : null;
        if (from.HasValue && to.HasValue && to <= from)
            throw new DomainException("Ledger end date must be after the start date.");

        var entries = await inventoryRepository.GetLedgerAsync(
            ingredientId,
            from,
            to,
            cancellationToken);
        return entries.Select(entry => entry.ToResponse()).ToList();
    }

    public async Task<MenuItemRecipeResponse> ReplaceMenuItemRecipeAsync(
        Guid menuItemId,
        ReplaceMenuItemRecipeRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = await menuItemRepository.GetByIdAsync(menuItemId, cancellationToken)
            ?? throw new NotFoundException($"Menu item with id '{menuItemId}' was not found.");

        var componentDefinitions = request.Components
            .Select(component => (component.IngredientId, component.Quantity))
            .ToList();
        var ingredientIds = componentDefinitions
            .Select(component => component.IngredientId)
            .Distinct()
            .ToList();
        var ingredients = await inventoryRepository.GetIngredientsByIdsAsync(
            ingredientIds,
            cancellationToken);

        if (ingredients.Count != ingredientIds.Count)
            throw new NotFoundException("One or more recipe ingredients were not found.");

        if (ingredients.Any(ingredient => !ingredient.IsActive))
            throw new DomainException("Inactive ingredients cannot be added to a recipe.");

        var recipe = await inventoryRepository.GetRecipeByMenuItemIdAsync(
            menuItemId,
            cancellationToken);

        if (recipe is null)
        {
            recipe = MenuItemRecipe.Create(menuItemId, componentDefinitions);
            await inventoryRepository.AddRecipeAsync(recipe, cancellationToken);
        }
        else
        {
            recipe.ReplaceComponents(componentDefinitions);
        }

        await inventoryRepository.SaveChangesAsync(cancellationToken);
        return await MapRecipeAsync(recipe, ingredients, cancellationToken);
    }

    public async Task<MenuItemRecipeResponse> GetMenuItemRecipeAsync(
        Guid menuItemId,
        CancellationToken cancellationToken = default)
    {
        var recipe = await inventoryRepository.GetRecipeByMenuItemIdAsync(
            menuItemId,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Recipe for menu item with id '{menuItemId}' was not found.");

        var ingredients = await inventoryRepository.GetIngredientsByIdsAsync(
            recipe.Components.Select(component => component.IngredientId).ToList(),
            cancellationToken);
        return await MapRecipeAsync(recipe, ingredients, cancellationToken);
    }

    public async Task<MenuAddonRecipeResponse> ReplaceMenuAddonRecipeAsync(
        Guid menuAddonId,
        ReplaceMenuItemRecipeRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = await menuAddonRepository.GetByIdAsync(menuAddonId, cancellationToken)
            ?? throw new NotFoundException($"Menu add-on with id '{menuAddonId}' was not found.");

        var componentDefinitions = request.Components
            .Select(component => (component.IngredientId, component.Quantity))
            .ToList();
        var ingredientIds = componentDefinitions
            .Select(component => component.IngredientId)
            .Distinct()
            .ToList();
        var ingredients = await inventoryRepository.GetIngredientsByIdsAsync(
            ingredientIds,
            cancellationToken);

        if (ingredients.Count != ingredientIds.Count)
            throw new NotFoundException("One or more recipe ingredients were not found.");

        if (ingredients.Any(ingredient => !ingredient.IsActive))
            throw new DomainException("Inactive ingredients cannot be added to a recipe.");

        var recipe = await inventoryRepository.GetRecipeByMenuAddonIdAsync(
            menuAddonId,
            cancellationToken);

        if (recipe is null)
        {
            recipe = MenuAddonRecipe.Create(menuAddonId, componentDefinitions);
            await inventoryRepository.AddMenuAddonRecipeAsync(recipe, cancellationToken);
        }
        else
        {
            recipe.ReplaceComponents(componentDefinitions);
        }

        await inventoryRepository.SaveChangesAsync(cancellationToken);
        return await MapAddonRecipeAsync(recipe, ingredients, cancellationToken);
    }

    public async Task<MenuAddonRecipeResponse> GetMenuAddonRecipeAsync(
        Guid menuAddonId,
        CancellationToken cancellationToken = default)
    {
        var recipe = await inventoryRepository.GetRecipeByMenuAddonIdAsync(
            menuAddonId,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Recipe for menu add-on with id '{menuAddonId}' was not found.");

        var ingredients = await inventoryRepository.GetIngredientsByIdsAsync(
            recipe.Components.Select(component => component.IngredientId).ToList(),
            cancellationToken);
        return await MapAddonRecipeAsync(recipe, ingredients, cancellationToken);
    }

    private async Task<MeasurementUnit> GetActiveUnitAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var unit = await inventoryRepository.GetUnitByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Measurement unit with id '{id}' was not found.");

        if (!unit.IsActive)
            throw new DomainException("Inactive measurement units cannot be assigned to ingredients.");

        return unit;
    }

    private async Task<MenuItemRecipeResponse> MapRecipeAsync(
        MenuItemRecipe recipe,
        IReadOnlyCollection<Ingredient> ingredients,
        CancellationToken cancellationToken)
    {
        var ingredientMap = ingredients.ToDictionary(ingredient => ingredient.Id);
        var unitMap = await GetUnitMapAsync(cancellationToken);
        return recipe.ToResponse(ingredientMap, unitMap);
    }

    private async Task<MenuAddonRecipeResponse> MapAddonRecipeAsync(
        MenuAddonRecipe recipe,
        IReadOnlyCollection<Ingredient> ingredients,
        CancellationToken cancellationToken)
    {
        var ingredientMap = ingredients.ToDictionary(ingredient => ingredient.Id);
        var unitMap = await GetUnitMapAsync(cancellationToken);
        return recipe.ToResponse(ingredientMap, unitMap);
    }

    private async Task<IReadOnlyDictionary<Guid, MeasurementUnit>> GetUnitMapAsync(
        CancellationToken cancellationToken)
    {
        var units = await inventoryRepository.GetUnitsAsync(
            includeInactive: true,
            cancellationToken);
        return units.ToDictionary(unit => unit.Id);
    }

    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
