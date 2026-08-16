using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminOrOperator")]
[Route("api/modifier-groups")]
public class ModifierController(ModifierGroupService modifierGroupService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ModifierGroupResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateModifierGroup(CreateModifierGroupRequest request)
    {
        var response = await modifierGroupService.CreateAsync(request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ModifierGroupResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllModifierGroups() =>
        Ok(await modifierGroupService.GetAllAsync());

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ModifierGroupResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModifierGroupById(Guid id) =>
        Ok(await modifierGroupService.GetByIdAsync(id));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ModifierGroupResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateModifierGroup(Guid id, UpdateModifierGroupRequest request) =>
        Ok(await modifierGroupService.UpdateAsync(id, request));

    [HttpPost("{groupId:guid}/modifiers")]
    [ProducesResponseType(typeof(ModifierResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateModifier(Guid groupId, CreateModifierRequest request)
    {
        var response = await modifierGroupService.AddModifierAsync(groupId, request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{groupId:guid}/modifiers/{modifierId:guid}")]
    [ProducesResponseType(typeof(ModifierResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateModifier(Guid groupId, Guid modifierId, UpdateModifierRequest request) =>
        Ok(await modifierGroupService.UpdateModifierAsync(groupId, modifierId, request));

    [HttpDelete("{groupId:guid}/modifiers/{modifierId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveModifier(Guid groupId, Guid modifierId)
    {
        await modifierGroupService.RemoveModifierAsync(groupId, modifierId);
        return NoContent();
    }

    [HttpPatch("{groupId:guid}/modifiers/{modifierId:guid}/availability")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeModifierAvailability(Guid groupId, Guid modifierId, ChangeAvailabilityRequest request)
    {
        await modifierGroupService.ChangeModifierAvailabilityAsync(groupId, modifierId, request.IsAvailable);
        return NoContent();
    }

    [HttpPost("{groupId:guid}/menu-items/{menuItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AttachToMenuItem(Guid groupId, Guid menuItemId)
    {
        await modifierGroupService.AttachToMenuItemAsync(groupId, menuItemId);
        return NoContent();
    }

    [HttpDelete("{groupId:guid}/menu-items/{menuItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DetachFromMenuItem(Guid groupId, Guid menuItemId)
    {
        await modifierGroupService.DetachFromMenuItemAsync(groupId, menuItemId);
        return NoContent();
    }

    [HttpGet("menu-items/{menuItemId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ModifierGroupMenuItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModifierGroupsByMenuItem(Guid menuItemId) =>
        Ok(await modifierGroupService.GetByMenuItemAsync(menuItemId));
}
