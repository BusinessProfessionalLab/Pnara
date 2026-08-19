using Application.DTOs;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/invoices")]
public class InvoicesController(InvoiceService invoiceService) : ControllerBase
{
    [HttpPost("/api/orders/{orderId:guid}/invoice")]
    [Authorize(Policy = "perm:invoices.issue")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> IssueInvoice(Guid orderId, IssueInvoiceRequest request)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException();
        var response = await invoiceService.IssueInvoiceAsync(orderId, request, userId);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("{id:guid}/pay")]
    [Authorize(Policy = "perm:invoices.pay")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Pay(Guid id)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException();
        return Ok(await invoiceService.PayInvoiceAsync(id, userId));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "perm:invoices.cancel")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException();
        return Ok(await invoiceService.CancelInvoiceAsync(id, userId));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "perm:invoices.view")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await invoiceService.GetByIdAsync(id));

    [HttpGet]
    [Authorize(Policy = "perm:invoices.view")]
    [ProducesResponseType(typeof(IReadOnlyList<InvoiceListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] string? from, [FromQuery] string? to, [FromQuery] PaymentStatus? status) =>
        Ok(await invoiceService.GetListAsync(from, to, status));
}
