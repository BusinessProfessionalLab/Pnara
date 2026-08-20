using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateMeasurementUnitRequest(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(20)] string Symbol);

public record UpdateMeasurementUnitRequest(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(20)] string Symbol,
    bool IsActive);

public record CreateIngredientRequest(
    Guid MeasurementUnitId,
    [Required, MaxLength(200)] string Name,
    [Range(0, double.MaxValue)] decimal OpeningStock = 0,
    [Range(0, double.MaxValue)] decimal MinimumStock = 0);

public record UpdateIngredientRequest(
    Guid MeasurementUnitId,
    [Required, MaxLength(200)] string Name,
    [Range(0, double.MaxValue)] decimal MinimumStock,
    bool IsActive);

public record AdjustStockRequest(
    decimal QuantityChange,
    [MaxLength(500)] string? Note = null);

public record ReplaceMenuItemRecipeRequest(
    [Required, MinLength(1)] IReadOnlyList<RecipeComponentRequest> Components);

public record RecipeComponentRequest(
    Guid IngredientId,
    [Range(typeof(decimal), "0.001", "1000000000000")] decimal Quantity);
