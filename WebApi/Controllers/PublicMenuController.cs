using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/menu")]
public class PublicMenuController(MenuGroupService menuGroupService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PublicMenuResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicMenu() =>
        Ok(await menuGroupService.GetPublicMenuAsync());
}
