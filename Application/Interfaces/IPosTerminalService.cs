namespace Application.Interfaces;

public interface IPosTerminalService
{
    Task<PosPaymentResult> RequestPaymentAsync(
        decimal amount,
        string invoiceId,
        CancellationToken cancellationToken = default);
}

public sealed record PosPaymentResult(
    bool Succeeded,
    string Status,
    string? ReferenceNumber = null,
    string? ErrorMessage = null);
