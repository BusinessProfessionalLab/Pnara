using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public long OrderNumber { get; private set; }
    public OrderChannel Channel { get; private set; }
    public OrderStatus Status { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public string? TableNumber { get; private set; }
    public string? CustomerName { get; private set; }
    public string? CustomerPhoneNumber { get; private set; }
    public string? DeliveryAddressTitle { get; private set; }
    public string? DeliveryAddressLine { get; private set; }
    public string? DeliveryCity { get; private set; }
    public string? DeliveryPostalCode { get; private set; }
    public string? DeliveryPhoneNumber { get; private set; }
    public string? RejectionReason { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RegisteredAtUtc { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Order()
    {
    }

    public static Order CreatePosDraft(long orderNumber, Guid createdByUserId, string? tableNumber = null)
    {
        ValidateOrderNumber(orderNumber);

        if (createdByUserId == Guid.Empty)
            throw new DomainException("Order must be created by a valid user.");

        if (tableNumber?.Length > 20)
            throw new DomainException("Table number cannot exceed 20 characters.");

        return new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Channel = OrderChannel.Pos,
            Status = OrderStatus.Draft,
            CreatedByUserId = createdByUserId,
            TableNumber = tableNumber?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static Order CreateWebOrder(long orderNumber, Guid userId, string customerName, UserAddress address, IEnumerable<OrderItem> items)
    {
        ValidateOrderNumber(orderNumber);

        if (userId == Guid.Empty)
            throw new DomainException("Web order must belong to a valid user.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name cannot be empty.");

        if (address is null)
            throw new DomainException("Web order requires a delivery address.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Channel = OrderChannel.Web,
            Status = OrderStatus.PendingApproval,
            CreatedByUserId = userId,
            CustomerName = customerName.Trim(),
            CustomerPhoneNumber = address.PhoneNumber,
            DeliveryAddressTitle = address.Title,
            DeliveryAddressLine = address.AddressLine,
            DeliveryCity = address.City,
            DeliveryPostalCode = address.PostalCode,
            DeliveryPhoneNumber = address.PhoneNumber,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in items)
            order.AddItemInternal(item);

        if (order._items.Count == 0)
            throw new DomainException("Web order must contain at least one item.");

        return order;
    }

    public OrderItem AddItem(Guid menuItemId, string productName, Money unitPrice, int quantity)
    {
        EnsureItemsCanBeChanged();

        var item = OrderItem.Create(menuItemId, productName, unitPrice, quantity);
        AddItemInternal(item);
        return item;
    }

    public void RemoveItem(Guid orderItemId)
    {
        EnsureItemsCanBeChanged();

        var item = _items.FirstOrDefault(i => i.Id == orderItemId)
            ?? throw new DomainException("Order item was not found.");

        _items.Remove(item);
    }

    public void SetTableNumber(string? tableNumber)
    {
        if (tableNumber?.Length > 20)
            throw new DomainException("Table number cannot exceed 20 characters.");

        TableNumber = tableNumber?.Trim();
    }

    public void Register()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only draft orders can be registered.");

        EnsureHasItems();

        Status = OrderStatus.Registered;
        RegisteredAtUtc = DateTime.UtcNow;
        Raise(new OrderRegistered(Id, OrderNumber, Channel, RegisteredAtUtc.Value));
    }

    public void Approve(Guid reviewerId)
    {
        if (Status != OrderStatus.PendingApproval)
            throw new DomainException("Only pending external orders can be approved.");

        if (reviewerId == Guid.Empty)
            throw new DomainException("Order must be approved by a valid user.");

        EnsureHasItems();

        Status = OrderStatus.Registered;
        RegisteredAtUtc = DateTime.UtcNow;
        ReviewedByUserId = reviewerId;
        ReviewedAtUtc = RegisteredAtUtc;
        Raise(new OrderRegistered(Id, OrderNumber, Channel, RegisteredAtUtc.Value));
    }

    public void Reject(Guid reviewerId, string reason)
    {
        if (Status != OrderStatus.PendingApproval)
            throw new DomainException("Only pending external orders can be rejected.");

        if (reviewerId == Guid.Empty)
            throw new DomainException("Order must be rejected by a valid user.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Rejection reason cannot be empty.");

        Status = OrderStatus.Rejected;
        RejectionReason = reason.Trim();
        ReviewedByUserId = reviewerId;
        ReviewedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is not (OrderStatus.Draft or OrderStatus.Registered))
            throw new DomainException("Only draft or registered orders can be cancelled.");

        Status = OrderStatus.Cancelled;
    }

    public void ApplyPriceSnapshots(IReadOnlyDictionary<Guid, Money> currentPrices)
    {
        if (Status != OrderStatus.Registered)
            throw new DomainException("Prices can only be re-snapshotted right before finalizing.");

        foreach (var item in _items)
        {
            if (currentPrices.TryGetValue(item.MenuItemId, out var price))
                item.UpdatePriceSnapshot(price);
        }
    }

    public void MarkPaid()
    {
        if (Status != OrderStatus.Invoiced)
            throw new DomainException("Only invoiced orders can be marked as paid.");

        Status = OrderStatus.Paid;
    }

    public void CancelAfterInvoice()
    {
        if (Status != OrderStatus.Invoiced)
            throw new DomainException("Only invoiced orders can be cancelled after invoice cancellation.");

        Status = OrderStatus.Cancelled;
    }

    public Money CalculateSubTotal()
    {
        if (_items.Count == 0)
            return Money.Zero();

        var total = Money.Zero(_items[0].UnitPrice.Currency);
        foreach (var item in _items)
            total = total.Add(item.LineTotal);

        return total;
    }

    public LegacyInvoice IssueInvoice(long invoiceNumber, Money discount, decimal taxRate, Guid userId)
    {
        if (Status != OrderStatus.Registered)
            throw new DomainException("Only registered orders can be invoiced.");
        var invoice = LegacyInvoice.Create(this, invoiceNumber, discount, taxRate);
        Status = OrderStatus.Invoiced;
        Raise(new InvoiceIssued(Id, Id, invoiceNumber, invoice.GrandTotal.Amount, invoice.GrandTotal.Currency, DateTime.UtcNow));
        return invoice;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddItemInternal(OrderItem item)
    {
        var existing = _items.FirstOrDefault(i => i.MenuItemId == item.MenuItemId);
        if (existing is not null)
        {
            existing.IncreaseQuantity(item.Quantity);
            return;
        }

        _items.Add(item);
    }

    private void EnsureItemsCanBeChanged()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Order items can only be changed while the order is a draft.");
    }

    private void EnsureHasItems()
    {
        if (_items.Count == 0)
            throw new DomainException("Order cannot be processed without at least one item.");
    }

    private void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    private static void ValidateOrderNumber(long orderNumber)
    {
        if (orderNumber <= 0)
            throw new DomainException("Order number must be positive.");
    }
}
