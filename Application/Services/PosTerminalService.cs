using Application.Interfaces;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public sealed class PosTerminalService(
    IPosTerminalRepository terminalRepository,
    IEnumerable<IPosTerminalAdapter> adapters,
    ILogger<PosTerminalService> logger) : IPosTerminalService
{
    public async Task<PosPaymentResult> RequestPaymentAsync(
        decimal amount,
        string invoiceId,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return new(false, "InvalidAmount", ErrorMessage: "Payment amount must be positive.");

        var terminal = await terminalRepository.GetActiveAsync(cancellationToken);
        if (terminal is null)
            return new(false, "NotConfigured", ErrorMessage: "No active POS terminal is configured.");

        var adapter = adapters.FirstOrDefault(x =>
            x.Provider.Equals(terminal.Provider, StringComparison.OrdinalIgnoreCase) ||
            (terminal.Provider.Equals("TCP", StringComparison.OrdinalIgnoreCase) &&
             x.Provider.Equals("TCP", StringComparison.OrdinalIgnoreCase)));
        if (adapter is null)
        {
            logger.LogWarning("No POS adapter registered for provider {Provider}.", terminal.Provider);
            return new(false, "Unavailable", ErrorMessage: "No adapter is registered for the configured POS provider.");
        }

        return await adapter.RequestPaymentAsync(terminal, amount, invoiceId, cancellationToken);
    }
}
