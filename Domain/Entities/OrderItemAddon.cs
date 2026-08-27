using Domain.Exceptions;

namespace Domain.Entities;

public class OrderItemAddon
{
    public Guid Id { get; private set; }
    public Guid OrderItemId { get; private set; }
    public Guid ModifierId { get; private set; }
    public string AddonName { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    private OrderItemAddon()
    {
    }

    private OrderItemAddon(
        Guid modifierId,
        string addonName,
        decimal quantity,
        decimal unitPrice)
    {
        Id = Guid.NewGuid();
        ModifierId = modifierId;
        AddonName = addonName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = quantity * unitPrice;
    }

    public static OrderItemAddon Create(
        Guid modifierId,
        string addonName,
        decimal quantity,
        decimal unitPrice)
    {
        if (modifierId == Guid.Empty)
            throw new DomainException("Order item add-on must reference a valid modifier.");

        if (string.IsNullOrWhiteSpace(addonName))
            throw new DomainException("Order item add-on name cannot be empty.");

        if (quantity <= 0)
            throw new DomainException("Order item add-on quantity must be greater than zero.");

        if (unitPrice < 0)
            throw new DomainException("Order item add-on price cannot be negative.");

        return new OrderItemAddon(
            modifierId,
            addonName.Trim(),
            quantity,
            unitPrice);
    }

    internal void AssignToOrderItem(Guid orderItemId)
    {
        if (orderItemId == Guid.Empty)
            throw new DomainException("Order item ID cannot be empty.");

        if (OrderItemId != Guid.Empty && OrderItemId != orderItemId)
            throw new DomainException("Order item add-on already belongs to another order item.");

        OrderItemId = orderItemId;
    }
}
