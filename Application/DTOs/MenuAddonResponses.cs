namespace Application.DTOs;

public record MenuAddonResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    bool IsAvailable,
    int DisplayOrder,
    IReadOnlyList<Guid> ApplicableMenuItemIds);

public record MenuAddonRecipeResponse(
    Guid Id,
    Guid MenuAddonId,
    IReadOnlyList<MenuAddonRecipeComponentResponse> Components);

public record MenuAddonRecipeComponentResponse(
    Guid Id,
    Guid IngredientId,
    string IngredientName,
    MeasurementUnitResponse MeasurementUnit,
    decimal Quantity);
