using Domain.Exceptions;

namespace Domain.Entities;

public class InvoiceItem
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string ItemName { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

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
        LineTotal = quantity * unitPrice;
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

    internal void AssignToInvoice(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
            throw new DomainException("Invoice ID cannot be empty.");

        if (InvoiceId != Guid.Empty && InvoiceId != invoiceId)
            throw new DomainException("Invoice item already belongs to another invoice.");

        InvoiceId = invoiceId;
    }
}
