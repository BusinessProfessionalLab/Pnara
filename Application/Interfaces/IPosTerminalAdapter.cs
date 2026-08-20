using Domain.Entities;

namespace Application.Interfaces;

public interface IPosTerminalAdapter
{
    string Provider { get; }
    Task<PosPaymentResult> RequestPaymentAsync(
        PosTerminalDefinition terminal,
        decimal amount,
        string invoiceId,
        CancellationToken cancellationToken = default);
}
