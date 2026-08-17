using Domain.Exceptions;

namespace Domain.Entities;

public class Permission
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    private Permission()
    {
    }

    private Permission(string name, string? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }

    public static Permission Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Permission name cannot be empty.");

        if (name.Length > 100)
            throw new DomainException("Permission name cannot exceed 100 characters.");

        return new Permission(name.Trim(), description);
    }
}
