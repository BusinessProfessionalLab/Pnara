using Domain.Exceptions;

namespace Domain.Entities;

public class MenuItem
{
    public Guid Id { get; private set; }
    public Guid GroupId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsAvailable { get; private set; }
    public int DisplayOrder { get; private set; }

    private MenuItem()
    {
    }

    private MenuItem(Guid groupId, string name, string? description, decimal price, string? imageUrl, int displayOrder)
    {
        Id = Guid.NewGuid();
        GroupId = groupId;
        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        IsAvailable = true;
        DisplayOrder = displayOrder;
    }

    public static MenuItem Create(Guid groupId, string name, string? description, decimal price, string? imageUrl, int displayOrder)
    {
        if (groupId == Guid.Empty)
            throw new DomainException("Menu item must belong to a valid group.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Menu item name cannot be empty.");

        if (price < 0)
            throw new DomainException("Menu item price cannot be negative.");

        return new MenuItem(groupId, name.Trim(), description, price, imageUrl, displayOrder);
    }

    public void Update(string name, string? description, decimal price, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Menu item name cannot be empty.");

        if (price < 0)
            throw new DomainException("Menu item price cannot be negative.");

        Name = name.Trim();
        Description = description;
        Price = price;
        DisplayOrder = displayOrder;
    }

    public void SetImage(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Image URL cannot be empty.");

        ImageUrl = url.Trim();
    }

    public void RemoveImage()
    {
        ImageUrl = null;
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
