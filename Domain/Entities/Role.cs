using Domain.Exceptions;

namespace Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

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

    public void AssignPermission(Permission permission)
    {
        if (permission == null)
            throw new DomainException("Permission cannot be null.");

        if (HasPermission(permission.Id))
            throw new DomainException("This permission is already assigned to the role.");

        RolePermissions.Add(new RolePermission { RoleId = Id, PermissionId = permission.Id, Role = this, Permission = permission });
    }

    public void RevokePermission(Guid permissionId)
    {
        var rolePermission = RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId)
            ?? throw new DomainException("The role does not have this permission.");

        RolePermissions.Remove(rolePermission);
    }

    public bool HasPermission(Guid permissionId)
    {
        return RolePermissions.Any(rp => rp.PermissionId == permissionId);
    }

    public IReadOnlyList<Permission> GetPermissions()
    {
        return RolePermissions.Select(rp => rp.Permission).ToList().AsReadOnly();
    }
}
