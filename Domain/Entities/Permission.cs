using Domain.Exceptions;

namespace Domain.Entities;

public class Permission
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Group { get; private set; }
    public bool IsSystemPermission { get; private set; }

    private Permission()
    {
    }

    private Permission(string name, string? description, string? group, bool isSystemPermission)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Group = group;
        IsSystemPermission = isSystemPermission;
    }

    public static Permission Create(string name, string? description = null, string? group = null, bool isSystemPermission = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Permission name cannot be empty.");

        if (name.Length > 100)
            throw new DomainException("Permission name cannot exceed 100 characters.");

        if (group?.Length > 100)
            throw new DomainException("Permission group cannot exceed 100 characters.");

        return new Permission(name.Trim(), description, group?.Trim(), isSystemPermission);
    }

    public void UpdateDetails(string? description, string? group)
    {
        if (group?.Length > 100)
            throw new DomainException("Permission group cannot exceed 100 characters.");

        Description = description;
        Group = group?.Trim();
    }
}
