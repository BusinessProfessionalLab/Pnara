using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.GetUserId() ?? throw new InvalidCredentialsException();

        var response = await userService.GetByIdAsync(userId);
        return Ok(response);
    }

    [HttpPut("{id:guid}/role")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleRequest request) =>
        Ok(await userService.AssignRoleAsync(id, request.RoleId));
}

public record AssignRoleRequest(Guid RoleId);
