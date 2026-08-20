using Application.Interfaces;
using Domain.Entities;
using System.Net.Sockets;

namespace Infrastructure.Printing;

public class EscPosTcpPrinterClient : IReceiptPrinterClient
{
    public async Task SendAsync(
        PrinterDefinition printer,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default)
    {
        if (printer.ConnectionType != Domain.Enums.PrinterConnectionType.Tcp)
            throw new NotSupportedException(
                $"Printer connection type '{printer.ConnectionType}' is not supported.");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

        using var client = new TcpClient();
        await client.ConnectAsync(printer.Host, printer.Port, timeoutCts.Token);
        await using var stream = client.GetStream();
        await stream.WriteAsync(data, timeoutCts.Token);
        await stream.FlushAsync(timeoutCts.Token);
    }
}
