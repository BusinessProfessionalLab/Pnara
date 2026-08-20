namespace Domain.Exceptions;

public class InventoryConcurrencyException : DomainException
{
    public InventoryConcurrencyException(string message) : base(message)
    {
    }
}
