namespace Application.DTOs;

public record UserResponse(
    Guid Id,
    string PhoneNumber,
    string FirstName,
    string LastName,
    Guid RoleId,
    string RoleName,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyList<string> Permissions);
