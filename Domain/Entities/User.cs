using Domain.Exceptions;

namespace Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string PhoneNumber { get; private set; } = null!;
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

    private User(string phoneNumber, string passwordHash, string firstName, string lastName, Guid roleId)
    {
        Id = Guid.NewGuid();
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        RoleId = roleId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static User Register(string phoneNumber, string passwordHash, string firstName, string lastName, Guid roleId)
    {
        return new User(NormalizePhoneNumber(phoneNumber), passwordHash, firstName, lastName, roleId);
    }

    public static User CreateByAdmin(string phoneNumber, string passwordHash, string firstName, string lastName, Guid roleId)
    {
        return new User(NormalizePhoneNumber(phoneNumber), passwordHash, firstName, lastName, roleId);
    }

    public void UpdateProfile(string? phoneNumber, string? firstName, string? lastName, string? passwordHash)
    {
        if (!string.IsNullOrWhiteSpace(phoneNumber))
            PhoneNumber = NormalizePhoneNumber(phoneNumber);
        if (firstName is not null) FirstName = firstName.Trim();
        if (lastName is not null) LastName = lastName.Trim();
        if (passwordHash is not null) PasswordHash = passwordHash;
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

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Phone number cannot be empty.");

        var normalized = phoneNumber.Trim();
        if (normalized.Length < 7 || normalized.Length > 30)
            throw new DomainException("Phone number must be between 7 and 30 characters.");
        return normalized;
    }
}
