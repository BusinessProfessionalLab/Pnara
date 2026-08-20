using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize(Policy = "perm:manage")]
[Route("api/menu")]
public class MenuController(
    MenuGroupService menuGroupService,
    MenuItemService menuItemService,
    MenuAddonService menuAddonService) : ControllerBase
{
    [HttpPost("groups")]
    [ProducesResponseType(typeof(MenuGroupResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGroup(CreateMenuGroupRequest request)
    {
        var response = await menuGroupService.CreateAsync(request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("groups/{id:guid}")]
    [ProducesResponseType(typeof(MenuGroupResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateGroup(Guid id, UpdateMenuGroupRequest request) =>
        Ok(await menuGroupService.UpdateAsync(id, request));

    [HttpPatch("groups/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetGroupStatus(Guid id, ToggleStatusRequest request)
    {
        await menuGroupService.SetStatusAsync(id, request.IsActive);
        return NoContent();
    }

    [HttpGet("groups")]
    [ProducesResponseType(typeof(IReadOnlyList<MenuGroupResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroups([FromQuery] bool includeInactive = false) =>
        Ok(await menuGroupService.GetAllAsync(includeInactive));

    [HttpPost("groups/{groupId:guid}/items")]
    [ProducesResponseType(typeof(MenuItemResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateItem(Guid groupId, [FromForm] CreateMenuItemRequest request, IFormFile? file = null)
    {
        var itemRequest = request with { GroupId = groupId };

        MenuItemResponse response;
        if (file is not null && file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            response = await menuItemService.CreateWithImageAsync(itemRequest, stream, file.FileName, file.ContentType, file.Length);
        }
        else
        {
            response = await menuItemService.CreateAsync(itemRequest);
        }

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("items/{id:guid}")]
    [ProducesResponseType(typeof(MenuItemResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateItem(Guid id, UpdateMenuItemRequest request) =>
        Ok(await menuItemService.UpdateAsync(id, request));

    [HttpPatch("items/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetItemStatus(Guid id, ToggleStatusRequest request)
    {
        await menuItemService.SetStatusAsync(id, request.IsActive);
        return NoContent();
    }

    [HttpPatch("items/{id:guid}/availability")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeItemAvailability(Guid id, ChangeAvailabilityRequest request)
    {
        await menuItemService.ChangeAvailabilityAsync(id, request);
        return NoContent();
    }

    [HttpDelete("items/{id:guid}/image")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveItemImage(Guid id)
    {
        await menuItemService.RemoveImageAsync(id);
        return NoContent();
    }

    [HttpPost("items/{id:guid}/image")]
    [ProducesResponseType(typeof(MenuItemResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadItemImage(Guid id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new Domain.Exceptions.DomainException("No file provided.");

        using var stream = file.OpenReadStream();
        var response = await menuItemService.UploadImageAsync(id, stream, file.FileName, file.ContentType, file.Length);
        return Ok(response);
    }

    [HttpGet("groups/{groupId:guid}/items")]
    [ProducesResponseType(typeof(IReadOnlyList<MenuItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetItemsByGroup(Guid groupId) =>
        Ok(await menuItemService.GetByGroupAsync(groupId));

    [HttpPost("addons")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(MenuAddonResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAddon(
        CreateMenuAddonRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await menuAddonService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("addons/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(MenuAddonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAddon(
        Guid id,
        UpdateMenuAddonRequest request,
        CancellationToken cancellationToken = default) =>
        Ok(await menuAddonService.UpdateAsync(id, request, cancellationToken));

    [HttpPatch("addons/{id:guid}/availability")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeAddonAvailability(
        Guid id,
        ChangeAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        await menuAddonService.SetAvailabilityAsync(id, request.IsAvailable, cancellationToken);
        return NoContent();
    }

    [HttpGet("addons")]
    [ProducesResponseType(typeof(IReadOnlyList<MenuAddonResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAddons(
        [FromQuery] bool includeUnavailable = false,
        [FromQuery] Guid? menuItemId = null,
        CancellationToken cancellationToken = default) =>
        Ok(await menuAddonService.GetAllAsync(
            includeUnavailable,
            menuItemId,
            cancellationToken));

    [HttpPut("addons/{id:guid}/applicability")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(MenuAddonResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReplaceAddonApplicability(
        Guid id,
        ReplaceMenuAddonApplicabilityRequest request,
        CancellationToken cancellationToken = default) =>
        Ok(await menuAddonService.ReplaceApplicabilityAsync(
            id,
            request,
            cancellationToken));
}
