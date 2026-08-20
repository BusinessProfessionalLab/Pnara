using Domain.Exceptions;

namespace Domain.Entities;

public class InvoiceItem
{
    private readonly List<InvoiceItemAddon> _addons = [];

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string ItemName { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }
    public IReadOnlyCollection<InvoiceItemAddon> Addons => _addons.AsReadOnly();

    private InvoiceItem()
    {
    }

    private InvoiceItem(Guid menuItemId, string itemName, decimal quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        MenuItemId = menuItemId;
        ItemName = itemName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        RecalculateLineTotal();
    }

    public static InvoiceItem Create(Guid menuItemId, string itemName, decimal quantity, decimal unitPrice)
    {
        if (menuItemId == Guid.Empty)
            throw new DomainException("Invoice item must reference a valid menu item.");

        if (string.IsNullOrWhiteSpace(itemName))
            throw new DomainException("Invoice item name cannot be empty.");

        if (quantity <= 0)
            throw new DomainException("Invoice item quantity must be greater than zero.");

        if (unitPrice < 0)
            throw new DomainException("Invoice item price cannot be negative.");

        return new InvoiceItem(menuItemId, itemName.Trim(), quantity, unitPrice);
    }

    public void AddAddon(InvoiceItemAddon addon)
    {
        ArgumentNullException.ThrowIfNull(addon);
        addon.AssignToInvoiceItem(Id);
        _addons.Add(addon);
        RecalculateLineTotal();
    }

    internal void AssignToInvoice(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
            throw new DomainException("Invoice ID cannot be empty.");

        if (InvoiceId != Guid.Empty && InvoiceId != invoiceId)
            throw new DomainException("Invoice item already belongs to another invoice.");

        InvoiceId = invoiceId;
        foreach (var addon in _addons)
            addon.AssignToInvoiceItem(Id);
    }

    private void RecalculateLineTotal() =>
        LineTotal = Quantity * UnitPrice + _addons.Sum(addon => addon.LineTotal);
}
