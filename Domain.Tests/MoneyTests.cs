using Xunit;
using Domain.Constants;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Create_RoundsAmountToTwoDecimals()
    {
        var money = Money.Create(10.555m);

        Assert.Equal(10.56m, money.Amount);
        Assert.Equal(Currencies.IranianRial, money.Currency);
    }

    [Fact]
    public void Create_NegativeAmount_Throws()
    {
        Assert.Throws<DomainException>(() => Money.Create(-1m));
    }

    [Fact]
    public void Create_EmptyCurrency_Throws()
    {
        Assert.Throws<DomainException>(() => Money.Create(10m, " "));
    }

    [Fact]
    public void Add_SameCurrency_SumsAmounts()
    {
        var sum = Money.Create(100m).Add(Money.Create(50.5m));

        Assert.Equal(150.5m, sum.Amount);
    }

    [Fact]
    public void Add_DifferentCurrencies_Throws()
    {
        var rial = Money.Create(100m, "IRR");
        var dollar = Money.Create(10m, "USD");

        Assert.Throws<DomainException>(() => rial.Add(dollar));
    }

    [Fact]
    public void Subtract_DifferentCurrencies_Throws()
    {
        var rial = Money.Create(100m, "IRR");
        var dollar = Money.Create(10m, "USD");

        Assert.Throws<DomainException>(() => rial.Subtract(dollar));
    }

    [Fact]
    public void Multiply_CalculatesAmount()
    {
        var result = Money.Create(33.33m).Multiply(3);

        Assert.Equal(99.99m, result.Amount);
    }

    [Fact]
    public void Multiply_NegativeMultiplier_Throws()
    {
        Assert.Throws<DomainException>(() => Money.Create(10m).Multiply(-1));
    }

    [Fact]
    public void Percentage_CalculatesRoundedAmount()
    {
        var result = Money.Create(200m).Percentage(9m);

        Assert.Equal(18m, result.Amount);
    }

    [Fact]
    public void Percentage_OutOfRange_Throws()
    {
        Assert.Throws<DomainException>(() => Money.Create(200m).Percentage(101m));
        Assert.Throws<DomainException>(() => Money.Create(200m).Percentage(-1m));
    }

    [Fact]
    public void Equality_IncludesCurrency()
    {
        var rial = Money.Create(100m, "IRR");
        var dollar = Money.Create(100m, "USD");

        Assert.NotEqual(rial, dollar);
        Assert.True(rial != dollar);
        Assert.Equal(Money.Create(100m, "IRR"), rial);
    }

    [Fact]
    public void Comparison_ComparesAmounts()
    {
        var large = Money.Create(200m);
        var small = Money.Create(100m);

        Assert.True(large > small);
        Assert.True(small < large);
        Assert.True(large >= Money.Create(200m));
        Assert.True(small <= Money.Create(100m));
    }

    [Fact]
    public void Comparison_DifferentCurrencies_Throws()
    {
        var rial = Money.Create(100m, "IRR");
        var dollar = Money.Create(10m, "USD");

        Assert.Throws<DomainException>(() => rial > dollar);
    }
}
