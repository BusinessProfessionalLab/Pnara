using Application.Common;
using Application.DTOs;
using Application.Exceptions;
using Application.Mappers;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

public class RoleService(IRoleRepository roleRepository, IPermissionRepository permissionRepository)
{
    public async Task<RoleDto> CreateAsync(CreateRoleRequest request)
    {
        if (await roleRepository.ExistsByNameAsync(request.Name))
            throw new RoleAlreadyExistsException();

        var role = Role.Create(request.Name, request.Description);

        await roleRepository.AddAsync(role);
        await roleRepository.SaveChangesAsync();

        return role.ToDto();
    }

    public async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleRequest request)
    {
        var role = await roleRepository.GetByIdAsync(id)
            ?? throw new RoleNotFoundException($"Role with id '{id}' was not found.");

        if (role.Name != request.Name && await roleRepository.ExistsByNameAsync(request.Name))
            throw new RoleAlreadyExistsException();

        role.UpdateDetails(request.Name, request.Description);

        await roleRepository.SaveChangesAsync();

        return role.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        var role = await roleRepository.GetByIdAsync(id)
            ?? throw new RoleNotFoundException($"Role with id '{id}' was not found.");

        if (role.IsSystemRole)
            throw new SystemRoleCannotBeDeletedException();

        if (await roleRepository.HasUsersAsync(id))
            throw new RoleInUseException();

        await roleRepository.Delete(role);
        await roleRepository.SaveChangesAsync();
    }

    public async Task<RoleDto> GetByIdAsync(Guid id)
    {
        var role = await roleRepository.GetByIdAsync(id)
            ?? throw new RoleNotFoundException($"Role with id '{id}' was not found.");

        return role.ToDto();
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync()
    {
        var roles = await roleRepository.GetAllAsync();
        return roles.Select(role => role.ToDto()).ToList();
    }

    public async Task<Result> AssignPermissionsToRoleAsync(Guid roleId, Guid[] permissionIds)
    {
        var role = await roleRepository.GetWithPermissionsAsync(roleId)
            ?? throw new RoleNotFoundException($"Role with id '{roleId}' was not found.");

        foreach (var permissionId in permissionIds)
        {
            if (role.HasPermission(permissionId))
                continue;

            var permission = await permissionRepository.GetByIdAsync(permissionId)
                ?? throw new PermissionNotFoundException($"Permission with id '{permissionId}' was not found.");

            role.AssignPermission(permission);
        }

        await roleRepository.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RevokePermissionFromRoleAsync(Guid roleId, Guid permissionId)
    {
        var role = await roleRepository.GetWithPermissionsAsync(roleId)
            ?? throw new RoleNotFoundException($"Role with id '{roleId}' was not found.");

        role.RevokePermission(permissionId);

        await roleRepository.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<PermissionDto>>> GetRolePermissionsAsync(Guid roleId)
    {
        var role = await roleRepository.GetWithPermissionsAsync(roleId)
            ?? throw new RoleNotFoundException($"Role with id '{roleId}' was not found.");

        var permissions = role.GetPermissions().Select(p => p.ToDto()).ToList();
        return Result<IReadOnlyList<PermissionDto>>.Success(permissions);
    }
}
