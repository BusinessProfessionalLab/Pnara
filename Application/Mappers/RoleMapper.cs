using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class RoleMapper
{
    public static RoleDto ToDto(this Role role) =>
        new(role.Id, role.Name, role.Description, role.IsSystemRole, role.CreatedAt);
}
