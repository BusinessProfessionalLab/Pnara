using Domain.Exceptions;

namespace Domain.Entities;

public class MenuGroup
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private MenuGroup()
    {
    }

    private MenuGroup(string name, int displayOrder)
    {
        Id = Guid.NewGuid();
        Name = name;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public static MenuGroup Create(string name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Menu group name cannot be empty.");

        return new MenuGroup(name.Trim(), displayOrder);
    }

    public void Update(string name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Menu group name cannot be empty.");

        Name = name.Trim();
        DisplayOrder = displayOrder;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
