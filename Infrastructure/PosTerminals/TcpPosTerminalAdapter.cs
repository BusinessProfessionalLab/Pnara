using System.Globalization;
using System.Net.Sockets;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.PosTerminals;

public sealed class TcpPosTerminalAdapter(ILogger<TcpPosTerminalAdapter> logger) : IPosTerminalAdapter
{
    public string Provider => "TCP";

    public async Task<PosPaymentResult> RequestPaymentAsync(
        PosTerminalDefinition terminal,
        decimal amount,
        string invoiceId,
        CancellationToken cancellationToken = default)
    {
        if (terminal.ConnectionType != PosTerminalConnectionType.Tcp ||
            string.IsNullOrWhiteSpace(terminal.Host) ||
            !terminal.Port.HasValue)
            return new(false, "InvalidConfiguration", ErrorMessage: "TCP terminal configuration is incomplete.");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(terminal.TimeoutSeconds));

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(terminal.Host, terminal.Port.Value, timeout.Token);
            await using var stream = client.GetStream();
            var payload = Encoding.ASCII.GetBytes(
                $"PAY|{invoiceId}|{amount.ToString("0.##", CultureInfo.InvariantCulture)}\n");
            await stream.WriteAsync(payload, timeout.Token);

            var buffer = new byte[512];
            var read = await stream.ReadAsync(buffer, timeout.Token);
            var response = Encoding.ASCII.GetString(buffer, 0, read).Trim();
            var parts = response.Split('|', 3, StringSplitOptions.TrimEntries);
            if (parts.Length >= 2 && parts[0].Equals("OK", StringComparison.OrdinalIgnoreCase))
                return new(true, "Succeeded", parts[1]);
            if (parts.Length >= 2 && parts[0].Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
                return new(false, "Cancelled", ErrorMessage: parts[1]);
            return new(false, "Failed", ErrorMessage: parts.Length > 1 ? parts[1] : "Terminal rejected the payment.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new(false, "TimedOut", ErrorMessage: "POS terminal request timed out.");
        }
        catch (Exception exception) when (exception is SocketException or IOException)
        {
            logger.LogWarning(exception, "POS terminal connection failed for {Host}:{Port}.", terminal.Host, terminal.Port);
            return new(false, "Unknown", ErrorMessage: "Unable to connect to the POS terminal.");
        }
    }
}
