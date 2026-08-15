using Application.DTOs;
using Application.Exceptions;
using Application.Mappers;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

public class RoleService(IRoleRepository roleRepository)
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
}
