using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class PermissionMapper
{
    public static PermissionDto ToDto(this Permission permission) =>
        new(permission.Id, permission.Name, permission.Description);
}
