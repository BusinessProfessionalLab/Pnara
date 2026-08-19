using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/user/addresses")]
public class UserAddressesController(UserAddressService userAddressService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserAddressResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException();
        return Ok(await userAddressService.GetAllAsync(userId));
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserAddressResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateUserAddressRequest request)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException();
        var response = await userAddressService.CreateAsync(userId, request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserAddressResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, UpdateUserAddressRequest request)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException();
        return Ok(await userAddressService.UpdateAsync(userId, id, request));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException();
        await userAddressService.DeleteAsync(userId, id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException();
        await userAddressService.SetDefaultAsync(userId, id);
        return NoContent();
    }
}
