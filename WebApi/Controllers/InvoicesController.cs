using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("{id:guid}")]
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
