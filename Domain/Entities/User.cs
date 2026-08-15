using Domain.Exceptions;

namespace Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    private User(string email, string passwordHash, string firstName, string lastName, Guid roleId)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        RoleId = roleId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static User Register(string email, string passwordHash, string firstName, string lastName, Guid roleId)
    {
        var normalizedEmail = ValidateAndNormalizeEmail(email);
        return new User(normalizedEmail, passwordHash, firstName, lastName, roleId);
    }

    public static User CreateByAdmin(string email, string passwordHash, string firstName, string lastName, Guid roleId)
    {
        var normalizedEmail = ValidateAndNormalizeEmail(email);
        return new User(normalizedEmail, passwordHash, firstName, lastName, roleId);
    }

    public void ChangeRole(Guid newRoleId)
    {
        if (newRoleId == Guid.Empty)
            throw new DomainException("Role ID cannot be empty.");

        RoleId = newRoleId;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    private static string ValidateAndNormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");

        return email.Trim().ToLowerInvariant();
    }
}
