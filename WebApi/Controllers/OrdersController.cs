using Application.DTOs;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersController(OrderService orderService) : ControllerBase
{
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "perm:orders.create")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await orderService.CancelAsync(id);
        return NoContent();
    }

    [HttpPut("{id:guid}/table-number")]
    [Authorize(Policy = "perm:orders.create")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetTableNumber(Guid id, [FromBody] SetTableNumberRequest request) =>
        Ok(await orderService.SetTableNumberAsync(id, request.TableNumber));

    [HttpGet("queue")]
    [Authorize(Policy = "perm:orders.view")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQueue([FromQuery] OrderStatus? status) =>
        Ok(await orderService.GetQueueAsync(status));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "perm:orders.view")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await orderService.GetByIdAsync(id));

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "perm:orders.approve")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var reviewerId = User.GetUserId() ?? throw new UnauthorizedAccessException();
        return Ok(await orderService.ApproveAsync(id, reviewerId));
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "perm:orders.approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reject(Guid id, RejectOrderRequest request)
    {
        var reviewerId = User.GetUserId() ?? throw new UnauthorizedAccessException();
        await orderService.RejectAsync(id, reviewerId, request.Reason);
        return NoContent();
    }

    [HttpPost("register")]
    [Authorize(Policy = "perm:orders.create")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterOrder(RegisterOrderRequest request)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException();
        var response = await orderService.RegisterOrderAsync(request, userId);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
