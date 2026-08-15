using Application.DTOs;
using Application.Exceptions;
using Application.Mappers;
using Domain.Repositories;

namespace Application.Services;

public class UserService(IUserRepository userRepository, IRoleRepository roleRepository)
{
    public async Task<IReadOnlyList<UserResponse>> GetUsersAsync(Guid? roleId = null)
    {
        var users = await userRepository.GetAllAsync(roleId);
        return users.Select(user => user.ToResponse()).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with id '{id}' was not found.");

        return user.ToResponse();
    }

    public async Task<UserResponse> AssignRoleAsync(Guid userId, Guid roleId)
    {
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with id '{userId}' was not found.");

        var role = await roleRepository.GetByIdAsync(roleId)
            ?? throw new RoleNotFoundException($"Role with id '{roleId}' was not found.");

        user.ChangeRole(roleId);

        await userRepository.SaveChangesAsync();

        return user.ToResponse();
    }
}
