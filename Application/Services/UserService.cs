using Application.DTOs;
using Application.Exceptions;
using Application.Mappers;
using Application.Interfaces;
using Domain.Repositories;

namespace Application.Services;

public class UserService(IUserRepository userRepository, IRoleRepository roleRepository, IPasswordHasher passwordHasher)
{
    public async Task<IReadOnlyList<UserResponse>> GetUsersAsync(Guid? roleId = null)
    {
        var users = await userRepository.GetAllAsync(roleId);
        var rolesWithPermissions = await roleRepository.GetAllWithPermissionsAsync();

        var permissionsByRole = rolesWithPermissions.ToDictionary(
            role => role.Id,
            role => (IReadOnlyList<string>)role.GetPermissions().Select(p => p.Name).ToList());

        return users
            .Select(user => user.ToResponse(
                permissionsByRole.TryGetValue(user.RoleId, out var permissions) ? permissions : []))
            .ToList();
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with id '{id}' was not found.");

        return user.ToResponse(await GetPermissionNamesAsync(user.RoleId));
    }

    public async Task<UserResponse> AssignRoleAsync(Guid userId, Guid roleId)
    {
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with id '{userId}' was not found.");

        var role = await roleRepository.GetByIdAsync(roleId)
            ?? throw new RoleNotFoundException($"Role with id '{roleId}' was not found.");

        if (role.Name == Domain.Constants.SystemRoles.Admin)
            throw new CannotAssignAdminRoleException("New Admin users cannot be created. Assign a different role.");

        if (user.Role is not null && user.Role.Name == Domain.Constants.SystemRoles.Admin)
            throw new CannotAssignAdminRoleException("The role of an Admin user cannot be changed.");

        user.ChangeRole(roleId);

        await userRepository.SaveChangesAsync();

        return user.ToResponse(await GetPermissionNamesAsync(roleId));
    }

    public async Task<UserResponse> UpdateAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with id '{userId}' was not found.");

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phoneNumber = request.PhoneNumber.Trim();
            var existing = await userRepository.GetByPhoneNumberAsync(phoneNumber);
            if (existing is not null && existing.Id != userId)
                throw new PhoneNumberAlreadyExistsException();
        }

        var passwordHash = string.IsNullOrWhiteSpace(request.Password) ? null : passwordHasher.Hash(request.Password);
        user.UpdateProfile(request.PhoneNumber, request.FirstName, request.LastName, passwordHash);
        await userRepository.SaveChangesAsync();
        return user.ToResponse(await GetPermissionNamesAsync(user.RoleId));
    }

    private async Task<IReadOnlyList<string>> GetPermissionNamesAsync(Guid roleId)
    {
        var role = await roleRepository.GetWithPermissionsAsync(roleId);
        return role?.GetPermissions().Select(p => p.Name).ToList() ?? [];
    }
}
