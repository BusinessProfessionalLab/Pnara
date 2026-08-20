using Domain.Enums;

namespace Application.DTOs;

public record MeasurementUnitResponse(
    Guid Id,
    string Name,
    string Symbol,
    bool IsActive);

public record IngredientResponse(
    Guid Id,
    string Name,
    MeasurementUnitResponse MeasurementUnit,
    decimal CurrentStock,
    decimal MinimumStock,
    bool IsLowStock,
    bool IsActive);

public record StockLedgerEntryResponse(
    Guid Id,
    Guid IngredientId,
    StockMovementType MovementType,
    decimal QuantityChange,
    decimal BalanceAfter,
    Guid? InvoiceId,
    string? Note,
    DateTime OccurredAtUtc);

public record MenuItemRecipeResponse(
    Guid Id,
    Guid MenuItemId,
    IReadOnlyList<RecipeComponentResponse> Components);

public record RecipeComponentResponse(
    Guid Id,
    Guid IngredientId,
    string IngredientName,
    MeasurementUnitResponse MeasurementUnit,
    decimal Quantity);
