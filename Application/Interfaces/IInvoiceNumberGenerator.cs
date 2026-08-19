namespace Application.Interfaces;

public interface IInvoiceNumberGenerator
{
    Task<long> NextAsync(CancellationToken cancellationToken = default);
}
