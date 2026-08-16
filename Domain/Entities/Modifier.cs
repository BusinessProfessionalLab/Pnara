using Domain.Exceptions;

namespace Domain.Entities;

public class Modifier
{
    public Guid Id { get; private set; }
    public Guid ModifierGroupId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public bool IsAvailable { get; private set; }
    public int DisplayOrder { get; private set; }

    private Modifier()
    {
    }

    private Modifier(Guid modifierGroupId, string name, decimal price, int displayOrder)
    {
        Id = Guid.NewGuid();
        ModifierGroupId = modifierGroupId;
        Name = name;
        Price = price;
        IsAvailable = true;
        DisplayOrder = displayOrder;
    }

    public static Modifier Create(Guid modifierGroupId, string name, decimal price, int displayOrder)
    {
        if (modifierGroupId == Guid.Empty)
            throw new DomainException("Modifier must belong to a valid modifier group.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Modifier name cannot be empty.");

        if (price < 0)
            throw new DomainException("Modifier price cannot be negative.");

        return new Modifier(modifierGroupId, name.Trim(), price, displayOrder);
    }

    public void Update(string name, decimal price, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Modifier name cannot be empty.");

        if (price < 0)
            throw new DomainException("Modifier price cannot be negative.");

        Name = name.Trim();
        Price = price;
        DisplayOrder = displayOrder;
    }

    public void ChangeAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }

    public void Activate()
    {
        IsAvailable = true;
    }

    public void Deactivate()
    {
        IsAvailable = false;
    }
}
