using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin")]
public class AdminController(AuthService authService, UserService userService) : ControllerBase
{
    [HttpPost("users")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser(CreateUserRequest request) =>
        StatusCode(StatusCodes.Status201Created, await authService.CreateUserByAdminAsync(request));

    [HttpGet("users")]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] Guid? roleId) =>
        Ok(await userService.GetUsersAsync(roleId));
}
