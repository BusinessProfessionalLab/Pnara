using Xunit;
using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Tests;

public class OrderItemTests
{
    private static readonly Guid MenuItemId = Guid.NewGuid();

    [Fact]
    public void Create_ValidInput_SetsSnapshotFields()
    {
        var item = OrderItem.Create(MenuItemId, "Pizza", Money.Create(120m), 2);

        Assert.Equal(MenuItemId, item.MenuItemId);
        Assert.Equal("Pizza", item.ProductName);
        Assert.Equal(120m, item.UnitPrice.Amount);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(240m, item.LineTotal.Amount);
    }

    [Fact]
    public void Create_EmptyMenuItemId_Throws()
    {
        Assert.Throws<DomainException>(() => OrderItem.Create(Guid.Empty, "Pizza", Money.Create(100m), 1));
    }

    [Fact]
    public void Create_EmptyProductName_Throws()
    {
        Assert.Throws<DomainException>(() => OrderItem.Create(MenuItemId, " ", Money.Create(100m), 1));
    }

    [Fact]
    public void Create_ZeroQuantity_Throws()
    {
        Assert.Throws<DomainException>(() => OrderItem.Create(MenuItemId, "Pizza", Money.Create(100m), 0));
    }

    [Fact]
    public void Create_NullUnitPrice_Throws()
    {
        Assert.Throws<DomainException>(() => OrderItem.Create(MenuItemId, "Pizza", null!, 1));
    }
}
