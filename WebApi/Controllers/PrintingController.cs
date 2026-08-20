using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/printing")]
public class PrintingController(
    ReceiptPrintingService receiptPrintingService,
    IReceiptPrintingService receiptPrintingFacade) : ControllerBase
{
    [HttpGet("printers")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(IReadOnlyList<PrinterResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrinters(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        Ok(await receiptPrintingService.GetPrintersAsync(
            includeInactive,
            cancellationToken));

    [HttpPost("printers")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PrinterResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePrinter(
        CreatePrinterRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await receiptPrintingService.CreatePrinterAsync(
            request,
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("printers/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PrinterResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePrinter(
        Guid id,
        UpdatePrinterRequest request,
        CancellationToken cancellationToken = default) =>
        Ok(await receiptPrintingService.UpdatePrinterAsync(
            id,
            request,
            cancellationToken));

    [HttpGet("templates")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(IReadOnlyList<ReceiptTemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        Ok(await receiptPrintingService.GetTemplatesAsync(
            includeInactive,
            cancellationToken));

    [HttpPut("templates/{receiptType}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ReceiptTemplateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertTemplate(
        ReceiptType receiptType,
        UpsertReceiptTemplateRequest request,
        CancellationToken cancellationToken = default) =>
        Ok(await receiptPrintingService.UpsertTemplateAsync(
            receiptType,
            request,
            cancellationToken));

    [HttpGet("mappings")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(IReadOnlyList<ReceiptPrinterMappingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMappings(
        CancellationToken cancellationToken = default) =>
        Ok(await receiptPrintingService.GetMappingsAsync(cancellationToken));

    [HttpPut("mappings/{receiptType}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ReceiptPrinterMappingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignPrinter(
        ReceiptType receiptType,
        AssignReceiptPrinterRequest request,
        CancellationToken cancellationToken = default) =>
        Ok(await receiptPrintingService.AssignPrinterAsync(
            receiptType,
            request,
            cancellationToken));

    [HttpPost("invoices/{invoiceId:guid}/{receiptType}")]
    [Authorize(Policy = "AdminOrOperator")]
    [ProducesResponseType(typeof(PrintReceiptResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PrintInvoice(
        Guid invoiceId,
        ReceiptType receiptType,
        CancellationToken cancellationToken = default) =>
        Ok(await receiptPrintingFacade.PrintAsync(
            invoiceId,
            receiptType,
            cancellationToken));
}
