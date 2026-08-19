using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/external-orders")]
public class ExternalOrdersController(OrderService orderService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Submit(SubmitExternalOrderRequest request)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException();
        var response = await orderService.SubmitExternalOrderAsync(userId, request);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
