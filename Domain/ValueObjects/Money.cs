using Domain.Constants;
using Domain.Exceptions;

namespace Domain.ValueObjects;

public class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = Currencies.IranianRial)
    {
        if (amount < 0)
            throw new DomainException("Money amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Money currency cannot be empty.");

        return new Money(Math.Round(amount, 2), currency.Trim().ToUpperInvariant());
    }

    public static Money Zero(string currency = Currencies.IranianRial) => Create(0m, currency);

    public Money Add(Money other)
    {
        AssertSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        AssertSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(int multiplier)
    {
        if (multiplier < 0)
            throw new DomainException("Money multiplier cannot be negative.");

        return new Money(Math.Round(Amount * multiplier, 2), Currency);
    }

    public Money Percentage(decimal percent)
    {
        if (percent < 0 || percent > 100)
            throw new DomainException("Percentage must be between 0 and 100.");

        return new Money(Math.Round(Amount * percent / 100m, 2), Currency);
    }

    private void AssertSameCurrency(Money other)
    {
        if (other is null)
            throw new DomainException("Money cannot be null.");

        if (Currency != other.Currency)
            throw new DomainException($"Cannot operate on different currencies ('{Currency}' and '{other.Currency}').");
    }

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() => $"{Amount:F2} {Currency}";

    public static bool operator ==(Money? left, Money? right) => Equals(left, right);

    public static bool operator !=(Money? left, Money? right) => !Equals(left, right);

    public static bool operator >(Money left, Money right)
    {
        if (left is null || right is null)
            throw new DomainException("Money cannot be null.");

        if (left.Currency != right.Currency)
            throw new DomainException($"Cannot compare different currencies ('{left.Currency}' and '{right.Currency}').");

        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right) => right > left;

    public static bool operator >=(Money left, Money right) => left == right || left > right;

    public static bool operator <=(Money left, Money right) => left == right || left < right;
}
