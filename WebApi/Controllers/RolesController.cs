using Application.Common;
using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/roles")]
public class RolesController(RoleService roleService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateRoleRequest request) =>
        StatusCode(StatusCodes.Status201Created, await roleService.CreateAsync(request));

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await roleService.GetAllAsync());

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await roleService.GetByIdAsync(id));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, UpdateRoleRequest request) =>
        Ok(await roleService.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await roleService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPut("{roleId:guid}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignPermissions(Guid roleId, AssignPermissionsRequest request)
    {
        var result = await roleService.AssignPermissionsToRoleAsync(roleId, request.PermissionIds);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return Ok();
    }

    [HttpDelete("{roleId:guid}/permissions/{permissionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokePermission(Guid roleId, Guid permissionId)
    {
        var result = await roleService.RevokePermissionFromRoleAsync(roleId, permissionId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return Ok();
    }

    [HttpGet("{roleId:guid}/permissions")]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolePermissions(Guid roleId)
    {
        var result = await roleService.GetRolePermissionsAsync(roleId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return Ok(result.Value);
    }
}
