using Application.DTOs;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminOrOperator")]
[Route("api/reports")]
public class ReportsController(SalesReportService salesReportService) : ControllerBase
{
    [HttpGet("sales")]
    [ProducesResponseType(typeof(SalesReportResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSales(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] SalesChannel? channel,
        [FromQuery] PaymentMethod? paymentMethod,
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await salesReportService.GetSalesAsync(
            fromUtc,
            toUtc,
            channel,
            paymentMethod,
            top,
            cancellationToken));
}
