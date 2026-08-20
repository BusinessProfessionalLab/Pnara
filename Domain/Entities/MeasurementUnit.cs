using Domain.Exceptions;

namespace Domain.Entities;

public class MeasurementUnit
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Symbol { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private MeasurementUnit()
    {
    }

    private MeasurementUnit(string name, string symbol)
    {
        Id = Guid.NewGuid();
        Name = name;
        Symbol = symbol;
        IsActive = true;
    }

    public static MeasurementUnit Create(string name, string symbol)
    {
        Validate(name, symbol);
        return new MeasurementUnit(name.Trim(), symbol.Trim());
    }

    public void Update(string name, string symbol, bool isActive)
    {
        Validate(name, symbol);

        Name = name.Trim();
        Symbol = symbol.Trim();
        IsActive = isActive;
    }

    private static void Validate(string name, string symbol)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Measurement unit name cannot be empty.");

        if (string.IsNullOrWhiteSpace(symbol))
            throw new DomainException("Measurement unit symbol cannot be empty.");
    }
}
