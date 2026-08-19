using Xunit;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Tests;

public class InvoiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static Order CreateRegisteredOrder(decimal unitPrice = 100m, int quantity = 2)
    {
        var order = Order.CreatePosDraft(1, UserId);
        order.AddItem(Guid.NewGuid(), "Pizza", Money.Create(unitPrice), quantity);
        order.Register();
        return order;
    }

    [Fact]
    public void Issue_CalculatesSubTotalDiscountTaxAndGrandTotal()
    {
        var order = Order.CreatePosDraft(1, UserId);
        order.AddItem(Guid.NewGuid(), "Pizza", Money.Create(100m), 2);
        order.AddItem(Guid.NewGuid(), "Salad", Money.Create(50m), 1);
        order.Register();

        var invoice = order.IssueInvoice(1, Money.Create(50m), 10m, UserId);

        Assert.Equal(250m, invoice.SubTotal.Amount);
        Assert.Equal(50m, invoice.Discount.Amount);
        Assert.Equal(20m, invoice.Tax.Amount);
        Assert.Equal(220m, invoice.GrandTotal.Amount);
        Assert.Equal(PaymentStatus.Pending, invoice.PaymentStatus);
    }

    [Fact]
    public void Issue_ZeroTaxAndDiscount_GrandTotalEqualsSubTotal()
    {
        var order = CreateRegisteredOrder();

        var invoice = order.IssueInvoice(1, Money.Zero(), 0m, UserId);

        Assert.Equal(200m, invoice.GrandTotal.Amount);
    }

    [Fact]
    public void Issue_DiscountGreaterThanSubTotal_Throws()
    {
        var order = CreateRegisteredOrder();

        Assert.Throws<DomainException>(() => order.IssueInvoice(1, Money.Create(201m), 0m, UserId));
    }

    [Fact]
    public void Issue_TaxRateOutOfRange_Throws()
    {
        var order = CreateRegisteredOrder();

        Assert.Throws<DomainException>(() => order.IssueInvoice(1, Money.Zero(), -1m, UserId));
        Assert.Throws<DomainException>(() => order.IssueInvoice(1, Money.Zero(), 101m, UserId));
    }

    [Fact]
    public void Issue_InvalidInvoiceNumber_Throws()
    {
        var order = CreateRegisteredOrder();

        Assert.Throws<DomainException>(() => order.IssueInvoice(0, Money.Zero(), 0m, UserId));
    }

    [Fact]
    public void Issue_InvoiceNumberIsStored()
    {
        var order = CreateRegisteredOrder();

        var invoice = order.IssueInvoice(42, Money.Zero(), 0m, UserId);

        Assert.Equal(42, invoice.InvoiceNumber);
    }

    [Fact]
    public void Pay_PendingInvoice_SetsPaidAndRaisesEvent()
    {
        var order = CreateRegisteredOrder();
        var invoice = order.IssueInvoice(1, Money.Zero(), 0m, UserId);

        invoice.Pay(UserId);

        Assert.Equal(PaymentStatus.Paid, invoice.PaymentStatus);
        Assert.NotNull(invoice.PaidAtUtc);
        Assert.Equal(UserId, invoice.PaidByUserId);
        var eventRaised = Assert.Single(invoice.DomainEvents);
        var paid = Assert.IsType<InvoicePaid>(eventRaised);
        Assert.Equal(invoice.Id, paid.InvoiceId);
    }

    [Fact]
    public void Pay_AlreadyPaidInvoice_Throws()
    {
        var order = CreateRegisteredOrder();
        var invoice = order.IssueInvoice(1, Money.Zero(), 0m, UserId);
        invoice.Pay(UserId);

        Assert.Throws<DomainException>(() => invoice.Pay(UserId));
    }

    [Fact]
    public void Pay_CancelledInvoice_Throws()
    {
        var order = CreateRegisteredOrder();
        var invoice = order.IssueInvoice(1, Money.Zero(), 0m, UserId);
        invoice.Cancel(UserId);

        Assert.Throws<DomainException>(() => invoice.Pay(UserId));
    }

    [Fact]
    public void Cancel_PendingInvoice_SetsCancelled()
    {
        var order = CreateRegisteredOrder();
        var invoice = order.IssueInvoice(1, Money.Zero(), 0m, UserId);

        invoice.Cancel(UserId);

        Assert.Equal(PaymentStatus.Cancelled, invoice.PaymentStatus);
        Assert.NotNull(invoice.CancelledAtUtc);
        Assert.Equal(UserId, invoice.CancelledByUserId);
    }

    [Fact]
    public void Cancel_PaidInvoice_Throws()
    {
        var order = CreateRegisteredOrder();
        var invoice = order.IssueInvoice(1, Money.Zero(), 0m, UserId);
        invoice.Pay(UserId);

        Assert.Throws<DomainException>(() => invoice.Cancel(UserId));
    }

    [Fact]
    public void Cancel_AlreadyCancelledInvoice_Throws()
    {
        var order = CreateRegisteredOrder();
        var invoice = order.IssueInvoice(1, Money.Zero(), 0m, UserId);
        invoice.Cancel(UserId);

        Assert.Throws<DomainException>(() => invoice.Cancel(UserId));
    }

    [Fact]
    public void PriceSnapshot_MenuPriceChangeDoesNotAffectIssuedInvoice()
    {
        var menuItem = MenuItem.Create(Guid.NewGuid(), "Pizza", null, 100m, null, 1);
        var order = Order.CreatePosDraft(1, UserId);
        order.AddItem(menuItem.Id, menuItem.Name, Money.Create(menuItem.Price), 2);
        order.Register();

        var invoice = order.IssueInvoice(1, Money.Zero(), 0m, UserId);

        menuItem.Update("Pizza", null, 150m, 1);

        Assert.Equal(200m, invoice.GrandTotal.Amount);
        Assert.Equal(200m, order.CalculateSubTotal().Amount);
    }

    [Fact]
    public void PriceSnapshot_ResnapshotBeforeIssue_UsesLatestMenuPrice()
    {
        var menuItem = MenuItem.Create(Guid.NewGuid(), "Pizza", null, 100m, null, 1);
        var order = Order.CreatePosDraft(1, UserId);
        order.AddItem(menuItem.Id, menuItem.Name, Money.Create(menuItem.Price), 2);
        order.Register();

        menuItem.Update("Pizza", null, 150m, 1);
        order.ApplyPriceSnapshots(new Dictionary<Guid, Money> { [menuItem.Id] = Money.Create(menuItem.Price) });

        var invoice = order.IssueInvoice(1, Money.Zero(), 0m, UserId);

        Assert.Equal(300m, invoice.GrandTotal.Amount);
    }
}
