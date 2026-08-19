using Domain.Exceptions;

namespace Domain.Entities;

public class UserAddress
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public string AddressLine { get; private set; } = null!;
    public string? City { get; private set; }
    public string PhoneNumber { get; private set; } = null!;
    public string? PostalCode { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private UserAddress()
    {
    }

    public static UserAddress Create(Guid userId, string title, string addressLine, string? city, string phoneNumber, string? postalCode, bool isDefault)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Address must belong to a valid user.");

        ValidateFields(title, addressLine, city, phoneNumber, postalCode);

        return new UserAddress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title.Trim(),
            AddressLine = addressLine.Trim(),
            City = city?.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            PostalCode = postalCode?.Trim(),
            IsDefault = isDefault,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string title, string addressLine, string? city, string phoneNumber, string? postalCode)
    {
        ValidateFields(title, addressLine, city, phoneNumber, postalCode);

        Title = title.Trim();
        AddressLine = addressLine.Trim();
        City = city?.Trim();
        PhoneNumber = phoneNumber.Trim();
        PostalCode = postalCode?.Trim();
    }

    public void SetAsDefault() => IsDefault = true;

    public void ClearDefault() => IsDefault = false;

    private static void ValidateFields(string title, string addressLine, string? city, string phoneNumber, string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Address title cannot be empty.");

        if (title.Length > 100)
            throw new DomainException("Address title cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(addressLine))
            throw new DomainException("Address line cannot be empty.");

        if (addressLine.Length > 1000)
            throw new DomainException("Address line cannot exceed 1000 characters.");

        if (city?.Length > 100)
            throw new DomainException("City cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Address phone number cannot be empty.");

        if (phoneNumber.Length > 30)
            throw new DomainException("Phone number cannot exceed 30 characters.");

        if (postalCode?.Length > 20)
            throw new DomainException("Postal code cannot exceed 20 characters.");
    }
}
