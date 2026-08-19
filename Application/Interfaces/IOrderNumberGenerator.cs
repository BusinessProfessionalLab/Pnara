namespace Application.Interfaces;

public interface IOrderNumberGenerator
{
    Task<long> NextAsync(CancellationToken cancellationToken = default);
}
