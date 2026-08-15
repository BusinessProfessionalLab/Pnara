using Domain.Exceptions;

namespace Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Role()
    {
    }

    private Role(string name, string? description, bool isSystemRole)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsSystemRole = isSystemRole;
        CreatedAt = DateTime.UtcNow;
    }

    public static Role Create(string name, string? description, bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name cannot be empty.");

        if (name.Length > 50)
            throw new DomainException("Role name cannot exceed 50 characters.");

        return new Role(name.Trim(), description, isSystemRole);
    }

    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name cannot be empty.");

        if (name.Length > 50)
            throw new DomainException("Role name cannot exceed 50 characters.");

        if (IsSystemRole && Name != name.Trim())
            throw new DomainException("System role name cannot be changed.");

        Name = name.Trim();
        Description = description;
    }
}
