using Application.DTOs;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminOrOperator")]
[Route("api/invoices")]
public class InvoicesController(InvoiceService invoiceService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await invoiceService.CreateAsync(request, cancellationToken);
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
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default) =>
        Ok(await invoiceService.GetByIdAsync(id, cancellationToken));

    [HttpPost("{id:guid}/settle")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Settle(
        Guid id,
        FinalizeInvoiceRequest request,
        CancellationToken cancellationToken = default) =>
        Ok(await invoiceService.FinalizeAsync(id, request, cancellationToken));
}
