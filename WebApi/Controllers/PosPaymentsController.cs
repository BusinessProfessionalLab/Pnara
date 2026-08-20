using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminOrOperator")]
[Route("api/invoices")]
public class PosPaymentsController(InvoiceService invoiceService) : ControllerBase
{
    [HttpPost("{id:guid}/card-payment")]
    public async Task<IActionResult> RequestPayment(
        Guid id,
        CancellationToken cancellationToken = default) =>
        Ok(await invoiceService.RequestCardPaymentAsync(id, cancellationToken));
}
