using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class OrderItem
{
    private readonly List<OrderItemAddon> _addons = [];

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }
    public DateTime AddedAtUtc { get; private set; }

    public IReadOnlyCollection<OrderItemAddon> Addons => _addons.AsReadOnly();

    public Money LineTotal => UnitPrice.Multiply(Quantity).Add(Money.Create(AddonLineTotal, UnitPrice.Currency));

    private decimal AddonLineTotal => _addons.Sum(addon => addon.LineTotal);

    private OrderItem()
    {
    }

    public static OrderItem Create(Guid menuItemId, string productName, Money unitPrice, int quantity)
    {
        if (menuItemId == Guid.Empty)
            throw new DomainException("Order item must reference a valid menu item.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Order item product name cannot be empty.");

        if (unitPrice is null)
            throw new DomainException("Order item unit price cannot be null.");

        if (quantity < 1)
            throw new DomainException("Order item quantity must be at least one.");

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            MenuItemId = menuItemId,
            ProductName = productName.Trim(),
            UnitPrice = unitPrice,
            Quantity = quantity,
            AddedAtUtc = DateTime.UtcNow
        };
    }

    public static OrderItem CreateWithAddons(Guid menuItemId, string productName, Money unitPrice, int quantity, IEnumerable<OrderItemAddon> addons)
    {
        var item = Create(menuItemId, productName, unitPrice, quantity);
        foreach (var addon in addons)
            item.AddAddon(addon);
        return item;
    }

    public void AddAddon(OrderItemAddon addon)
    {
        ArgumentNullException.ThrowIfNull(addon);
        addon.AssignToOrderItem(Id);
        _addons.Add(addon);
    }

    internal void IncreaseQuantity(int quantity)
    {
        if (quantity < 1)
            throw new DomainException("Order item quantity must be at least one.");

        Quantity += quantity;
    }

    internal void UpdatePriceSnapshot(Money price)
    {
        UnitPrice = price ?? throw new DomainException("Order item unit price cannot be null.");
    }
}
