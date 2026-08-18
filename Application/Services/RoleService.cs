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

        var permissions = await ResolvePermissionsAsync(request.PermissionIds);
        var role = Role.Create(request.Name, request.Description);

        foreach (var permission in permissions)
            role.AssignPermission(permission);

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
        var role = await roleRepository.GetWithPermissionsAsync(id)
            ?? throw new RoleNotFoundException($"Role with id '{id}' was not found.");

        return role.ToDto();
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync()
    {
        var roles = await roleRepository.GetAllWithPermissionsAsync();
        return roles.Select(role => role.ToDto()).ToList();
    }

    public async Task<Result> AssignPermissionsToRoleAsync(Guid roleId, Guid[] permissionIds)
    {
        var role = await GetEditableRoleWithPermissionsAsync(roleId);

        var permissions = await ResolvePermissionsAsync(permissionIds);
        foreach (var permission in permissions)
        {
            if (!role.HasPermission(permission.Id))
                role.AssignPermission(permission);
        }

        await roleRepository.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RevokePermissionFromRoleAsync(Guid roleId, Guid permissionId)
    {
        var role = await GetEditableRoleWithPermissionsAsync(roleId);

        if (role.HasPermission(permissionId))
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

    private async Task<Role> GetEditableRoleWithPermissionsAsync(Guid roleId)
    {
        var role = await roleRepository.GetWithPermissionsAsync(roleId)
            ?? throw new RoleNotFoundException($"Role with id '{roleId}' was not found.");

        if (role.IsSystemRole)
            throw new SystemRoleCannotBeModifiedException("Permissions of the system Admin role cannot be changed.");

        return role;
    }

    private async Task<IReadOnlyList<Permission>> ResolvePermissionsAsync(IReadOnlyList<Guid>? permissionIds)
    {
        if (permissionIds is null || permissionIds.Count == 0)
            return [];

        var distinctIds = permissionIds.Distinct().ToList();
        var permissions = await permissionRepository.GetByIdsAsync(distinctIds);

        if (permissions.Count != distinctIds.Count)
            throw new PermissionNotFoundException("One or more permissions were not found.");

        return permissions;
    }
}
