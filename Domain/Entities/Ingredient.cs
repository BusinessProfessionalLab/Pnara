using Domain.Exceptions;

namespace Domain.Entities;

public class Ingredient
{
    public Guid Id { get; private set; }
    public Guid MeasurementUnitId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal CurrentStock { get; private set; }
    public decimal MinimumStock { get; private set; }
    public bool IsActive { get; private set; }
    public Guid ConcurrencyToken { get; private set; }
    public bool IsLowStock => CurrentStock < MinimumStock;

    private Ingredient()
    {
    }

    private Ingredient(
        Guid measurementUnitId,
        string name,
        decimal openingStock,
        decimal minimumStock)
    {
        Id = Guid.NewGuid();
        MeasurementUnitId = measurementUnitId;
        Name = name;
        CurrentStock = openingStock;
        MinimumStock = minimumStock;
        IsActive = true;
        ConcurrencyToken = Guid.NewGuid();
    }

    public static Ingredient Create(
        Guid measurementUnitId,
        string name,
        decimal openingStock = 0,
        decimal minimumStock = 0)
    {
        Validate(measurementUnitId, name, minimumStock);

        if (openingStock < 0)
            throw new DomainException("Opening stock cannot be negative.");

        return new Ingredient(
            measurementUnitId,
            name.Trim(),
            openingStock,
            minimumStock);
    }

    public void Update(
        Guid measurementUnitId,
        string name,
        decimal minimumStock,
        bool isActive)
    {
        Validate(measurementUnitId, name, minimumStock);

        if (MeasurementUnitId != measurementUnitId && CurrentStock != 0)
            throw new DomainException("The measurement unit can only be changed when stock is zero.");

        MeasurementUnitId = measurementUnitId;
        Name = name.Trim();
        MinimumStock = minimumStock;
        IsActive = isActive;
    }

    public decimal AdjustStock(decimal quantityChange)
    {
        if (quantityChange == 0)
            throw new DomainException("Stock adjustment cannot be zero.");

        var newBalance = CurrentStock + quantityChange;
        if (newBalance < 0)
            throw new DomainException($"Insufficient stock for ingredient '{Name}'.");

        CurrentStock = newBalance;
        ConcurrencyToken = Guid.NewGuid();

        return CurrentStock;
    }

    public decimal Consume(decimal quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Consumption quantity must be greater than zero.");

        return AdjustStock(-quantity);
    }

    private static void Validate(Guid measurementUnitId, string name, decimal minimumStock)
    {
        if (measurementUnitId == Guid.Empty)
            throw new DomainException("Ingredient must reference a valid measurement unit.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Ingredient name cannot be empty.");

        if (minimumStock < 0)
            throw new DomainException("Minimum stock cannot be negative.");
    }
}
