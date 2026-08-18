using Application.DTOs;
using Application.Exceptions;
using Application.Mappers;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

public class PermissionService(IPermissionRepository permissionRepository)
{
    public async Task<IReadOnlyList<PermissionDto>> GetAllAsync()
    {
        var permissions = await permissionRepository.GetAllAsync();
        return permissions.Select(p => p.ToDto()).ToList();
    }

    public async Task<PermissionDto> CreateAsync(CreatePermissionRequest request)
    {
        if (await permissionRepository.ExistsByNameAsync(request.Name))
            throw new PermissionAlreadyExistsException();

        var permission = Permission.Create(request.Name, request.Description, request.Group, isSystemPermission: false);

        await permissionRepository.AddAsync(permission);
        await permissionRepository.SaveChangesAsync();

        return permission.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        var permission = await permissionRepository.GetByIdAsync(id)
            ?? throw new PermissionNotFoundException($"Permission with id '{id}' was not found.");

        if (permission.IsSystemPermission)
            throw new SystemPermissionCannotBeDeletedException();

        await permissionRepository.Remove(permission);
        await permissionRepository.SaveChangesAsync();
    }
}
