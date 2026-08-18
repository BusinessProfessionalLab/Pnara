using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class UserMapper
{
    public static UserResponse ToResponse(this User user, IReadOnlyList<string>? permissions = null) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.RoleId, user.Role.Name, user.IsActive, user.CreatedAt,
            permissions ?? []);
}
