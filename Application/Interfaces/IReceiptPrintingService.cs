using Application.DTOs;
using Domain.Enums;

namespace Application.Interfaces;

public interface IReceiptPrintingService
{
    Task<PrintReceiptResponse> PrintAsync(
        Guid invoiceId,
        ReceiptType receiptType,
        CancellationToken cancellationToken = default);
}
