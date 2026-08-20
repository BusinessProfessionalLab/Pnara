using Domain.Entities;

namespace Application.Interfaces;

public interface IReceiptPrinterClient
{
    Task SendAsync(
        PrinterDefinition printer,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default);
}
