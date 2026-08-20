using Domain.Exceptions;

namespace Domain.Entities;

public class MenuAddon
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public bool IsAvailable { get; private set; }
    public int DisplayOrder { get; private set; }

    private MenuAddon()
    {
    }

    private MenuAddon(
        string name,
        string? description,
        decimal price,
        int displayOrder)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        IsAvailable = true;
        DisplayOrder = displayOrder;
    }

    public static MenuAddon Create(
        string name,
        string? description,
        decimal price,
        int displayOrder)
    {
        Validate(name, price);
        return new MenuAddon(name.Trim(), description, price, displayOrder);
    }

    public void Update(
        string name,
        string? description,
        decimal price,
        int displayOrder)
    {
        Validate(name, price);
        Name = name.Trim();
        Description = description;
        Price = price;
        DisplayOrder = displayOrder;
    }

    public void ChangeAvailability(bool isAvailable) =>
        IsAvailable = isAvailable;

    private static void Validate(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Menu add-on name cannot be empty.");

        if (price < 0)
            throw new DomainException("Menu add-on price cannot be negative.");
    }
}
