using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin")]
public class AdminController(
    AuthService authService,
    UserService userService,
    IUserRepository userRepository) : ControllerBase
{
    [HttpPost("users")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var currentUser = await GetCurrentUserAsync() ?? throw new InvalidCredentialsException();

        var response = await authService.CreateUserByAdminAsync(request, currentUser);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] Guid? roleId) =>
        Ok(await userService.GetUsersAsync(roleId));

    private async Task<User?> GetCurrentUserAsync()
    {
        var userId = User.GetUserId();
        if (userId is null)
            return null;

        return await userRepository.GetByIdAsync(userId.Value);
    }
}
