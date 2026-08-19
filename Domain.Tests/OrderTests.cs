using Xunit;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Tests;

public class OrderTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid MenuItemId = Guid.NewGuid();

    private static Order CreatePosDraftWithItem()
    {
        var order = Order.CreatePosDraft(1, UserId);
        order.AddItem(MenuItemId, "Pizza", Money.Create(100m), 2);
        return order;
    }

    private static UserAddress CreateUserAddress() =>
        UserAddress.Create(Guid.NewGuid(), "Home", "Street 1", "Tehran", "09121112233", "1234567890", isDefault: true);

    [Fact]
    public void CreatePosDraft_SetsDraftStatusAndPosChannel()
    {
        var order = Order.CreatePosDraft(5, UserId);

        Assert.Equal(OrderStatus.Draft, order.Status);
        Assert.Equal(OrderChannel.Pos, order.Channel);
        Assert.Equal(5, order.OrderNumber);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void CreatePosDraft_InvalidOrderNumber_Throws()
    {
        Assert.Throws<DomainException>(() => Order.CreatePosDraft(0, UserId));
    }

    [Fact]
    public void AddItem_ToDraft_AddsItemWithPriceSnapshot()
    {
        var order = Order.CreatePosDraft(1, UserId);

        var item = order.AddItem(MenuItemId, "Pizza", Money.Create(120m), 3);

        var stored = Assert.Single(order.Items);
        Assert.Equal(item.Id, stored.Id);
        Assert.Equal(120m, stored.UnitPrice.Amount);
        Assert.Equal(360m, order.CalculateSubTotal().Amount);
    }

    [Fact]
    public void AddItem_SameMenuItemTwice_MergesQuantity()
    {
        var order = Order.CreatePosDraft(1, UserId);

        order.AddItem(MenuItemId, "Pizza", Money.Create(100m), 2);
        order.AddItem(MenuItemId, "Pizza", Money.Create(100m), 3);

        var item = Assert.Single(order.Items);
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public void AddItem_AfterRegistration_Throws()
    {
        var order = CreatePosDraftWithItem();
        order.Register();

        Assert.Throws<DomainException>(() => order.AddItem(Guid.NewGuid(), "Burger", Money.Create(50m), 1));
    }

    [Fact]
    public void RemoveItem_FromDraft_RemovesItem()
    {
        var order = CreatePosDraftWithItem();
        var itemId = order.Items[0].Id;

        order.RemoveItem(itemId);

        Assert.Empty(order.Items);
    }

    [Fact]
    public void RemoveItem_MissingItem_Throws()
    {
        var order = CreatePosDraftWithItem();

        Assert.Throws<DomainException>(() => order.RemoveItem(Guid.NewGuid()));
    }

    [Fact]
    public void RemoveItem_AfterRegistration_Throws()
    {
        var order = CreatePosDraftWithItem();
        order.Register();

        Assert.Throws<DomainException>(() => order.RemoveItem(order.Items[0].Id));
    }

    [Fact]
    public void Register_EmptyOrder_Throws()
    {
        var order = Order.CreatePosDraft(1, UserId);

        Assert.Throws<DomainException>(() => order.Register());
    }

    [Fact]
    public void Register_WithItems_SetsRegisteredAndRaisesEvent()
    {
        var order = CreatePosDraftWithItem();

        order.Register();

        Assert.Equal(OrderStatus.Registered, order.Status);
        Assert.NotNull(order.RegisteredAtUtc);
        var eventRaised = Assert.Single(order.DomainEvents);
        var registered = Assert.IsType<OrderRegistered>(eventRaised);
        Assert.Equal(order.Id, registered.OrderId);
        Assert.Equal(order.OrderNumber, registered.OrderNumber);
    }

    [Fact]
    public void Register_Twice_Throws()
    {
        var order = CreatePosDraftWithItem();
        order.Register();

        Assert.Throws<DomainException>(() => order.Register());
    }

    [Fact]
    public void CreateWebOrder_SetsPendingApprovalAndSnapshotsAddress()
    {
        var address = CreateUserAddress();
        var items = new[] { OrderItem.Create(MenuItemId, "Pizza", Money.Create(100m), 2) };

        var order = Order.CreateWebOrder(10, UserId, "Ali Rezaei", address, items);

        Assert.Equal(OrderStatus.PendingApproval, order.Status);
        Assert.Equal(OrderChannel.Web, order.Channel);
        Assert.Equal("Ali Rezaei", order.CustomerName);
        Assert.Equal(address.AddressLine, order.DeliveryAddressLine);
        Assert.Equal(address.PhoneNumber, order.DeliveryPhoneNumber);
        Assert.Single(order.Items);
    }

    [Fact]
    public void CreateWebOrder_WithoutItems_Throws()
    {
        Assert.Throws<DomainException>(() => Order.CreateWebOrder(10, UserId, "Ali Rezaei", CreateUserAddress(), []));
    }

    [Fact]
    public void Approve_PendingWebOrder_RegistersAndRaisesEvent()
    {
        var order = CreatePendingWebOrder();
        var reviewerId = Guid.NewGuid();

        order.Approve(reviewerId);

        Assert.Equal(OrderStatus.Registered, order.Status);
        Assert.Equal(reviewerId, order.ReviewedByUserId);
        Assert.NotNull(order.RegisteredAtUtc);
        Assert.Contains(order.DomainEvents, e => e is OrderRegistered);
    }

    [Fact]
    public void Approve_PosDraft_Throws()
    {
        var order = CreatePosDraftWithItem();

        Assert.Throws<DomainException>(() => order.Approve(Guid.NewGuid()));
    }

    [Fact]
    public void Reject_PendingWebOrder_KeepsItInDatabaseAsRejected()
    {
        var order = CreatePendingWebOrder();
        var reviewerId = Guid.NewGuid();

        order.Reject(reviewerId, "Out of stock");

        Assert.Equal(OrderStatus.Rejected, order.Status);
        Assert.Equal("Out of stock", order.RejectionReason);
        Assert.Equal(reviewerId, order.ReviewedByUserId);
        Assert.Single(order.Items);
    }

    [Fact]
    public void Reject_EmptyReason_Throws()
    {
        var order = CreatePendingWebOrder();

        Assert.Throws<DomainException>(() => order.Reject(Guid.NewGuid(), " "));
    }

    [Fact]
    public void RejectedOrder_IsTerminal()
    {
        var order = CreatePendingWebOrder();
        order.Reject(Guid.NewGuid(), "Out of stock");

        Assert.Throws<DomainException>(() => order.Approve(Guid.NewGuid()));
        Assert.Throws<DomainException>(() => order.Register());
        Assert.Throws<DomainException>(() => order.Cancel());
        Assert.Throws<DomainException>(() => order.Reject(Guid.NewGuid(), "Again"));
    }

    [Fact]
    public void Cancel_DraftOrRegistered_SetsCancelled()
    {
        var draft = CreatePosDraftWithItem();
        draft.Cancel();
        Assert.Equal(OrderStatus.Cancelled, draft.Status);

        var registered = CreatePosDraftWithItem();
        registered.Register();
        registered.Cancel();
        Assert.Equal(OrderStatus.Cancelled, registered.Status);
    }

    [Fact]
    public void Cancel_InvoicedOrder_Throws()
    {
        var order = CreateInvoicedOrder();

        Assert.Throws<DomainException>(() => order.Cancel());
    }

    [Fact]
    public void ApplyPriceSnapshots_UpdatesItemPrices()
    {
        var order = CreatePosDraftWithItem();
        order.Register();

        order.ApplyPriceSnapshots(new Dictionary<Guid, Money> { [MenuItemId] = Money.Create(150m) });

        Assert.Equal(150m, order.Items[0].UnitPrice.Amount);
        Assert.Equal(300m, order.CalculateSubTotal().Amount);
    }

    [Fact]
    public void ApplyPriceSnapshots_BeforeRegistration_Throws()
    {
        var order = CreatePosDraftWithItem();

        Assert.Throws<DomainException>(() => order.ApplyPriceSnapshots(new Dictionary<Guid, Money>()));
    }

    [Fact]
    public void IssueInvoice_FromRegistered_CreatesInvoiceAndRaisesEvent()
    {
        var order = CreatePosDraftWithItem();
        order.Register();

        var invoice = order.IssueInvoice(1, Money.Zero(), 10m, Guid.NewGuid());

        Assert.Equal(OrderStatus.Invoiced, order.Status);
        Assert.Equal(order.Id, invoice.OrderId);
        Assert.Contains(order.DomainEvents, e => e is InvoiceIssued);
    }

    [Fact]
    public void IssueInvoice_FromDraft_Throws()
    {
        var order = CreatePosDraftWithItem();

        Assert.Throws<DomainException>(() => order.IssueInvoice(1, Money.Zero(), 0m, Guid.NewGuid()));
    }

    [Fact]
    public void MarkPaid_OnlyFromInvoiced()
    {
        var order = CreateInvoicedOrder();

        order.MarkPaid();

        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void MarkPaid_FromRegistered_Throws()
    {
        var order = CreatePosDraftWithItem();
        order.Register();

        Assert.Throws<DomainException>(() => order.MarkPaid());
    }

    [Fact]
    public void CancelAfterInvoice_OnlyFromInvoiced()
    {
        var order = CreateInvoicedOrder();

        order.CancelAfterInvoice();

        Assert.Equal(OrderStatus.Cancelled, order.Status);

        var registered = CreatePosDraftWithItem();
        registered.Register();
        Assert.Throws<DomainException>(() => registered.CancelAfterInvoice());
    }

    [Fact]
    public void FullPosFlow_DraftToPaid()
    {
        var order = Order.CreatePosDraft(1, UserId);
        order.AddItem(MenuItemId, "Pizza", Money.Create(100m), 2);
        order.Register();
        var invoice = order.IssueInvoice(1, Money.Zero(), 0m, Guid.NewGuid());
        invoice.Pay(Guid.NewGuid());
        order.MarkPaid();

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(PaymentStatus.Paid, invoice.PaymentStatus);
    }

    [Fact]
    public void FullWebFlow_SubmitApproveIssueCancel()
    {
        var order = CreatePendingWebOrder();
        order.Approve(Guid.NewGuid());
        var invoice = order.IssueInvoice(1, Money.Zero(), 0m, Guid.NewGuid());
        invoice.Cancel(Guid.NewGuid());
        order.CancelAfterInvoice();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(PaymentStatus.Cancelled, invoice.PaymentStatus);
    }

    private static Order CreatePendingWebOrder()
    {
        var items = new[] { OrderItem.Create(MenuItemId, "Pizza", Money.Create(100m), 2) };
        return Order.CreateWebOrder(10, Guid.NewGuid(), "Ali Rezaei", CreateUserAddress(), items);
    }

    private static Order CreateInvoicedOrder()
    {
        var order = CreatePosDraftWithItem();
        order.Register();
        order.IssueInvoice(1, Money.Zero(), 0m, Guid.NewGuid());
        return order;
    }
}
