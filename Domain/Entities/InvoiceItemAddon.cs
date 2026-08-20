using Domain.Exceptions;

namespace Domain.Entities;

public class InvoiceItemAddon
{
    public Guid Id { get; private set; }
    public Guid InvoiceItemId { get; private set; }
    public Guid MenuAddonId { get; private set; }
    public string AddonName { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    private InvoiceItemAddon()
    {
    }

    private InvoiceItemAddon(
        Guid menuAddonId,
        string addonName,
        decimal quantity,
        decimal unitPrice)
    {
        Id = Guid.NewGuid();
        MenuAddonId = menuAddonId;
        AddonName = addonName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = quantity * unitPrice;
    }

    public static InvoiceItemAddon Create(
        Guid menuAddonId,
        string addonName,
        decimal quantity,
        decimal unitPrice)
    {
        if (menuAddonId == Guid.Empty)
            throw new DomainException("Invoice add-on must reference a valid menu add-on.");

        if (string.IsNullOrWhiteSpace(addonName))
            throw new DomainException("Invoice add-on name cannot be empty.");

        if (quantity <= 0)
            throw new DomainException("Invoice add-on quantity must be greater than zero.");

        if (unitPrice < 0)
            throw new DomainException("Invoice add-on price cannot be negative.");

        return new InvoiceItemAddon(
            menuAddonId,
            addonName.Trim(),
            quantity,
            unitPrice);
    }

    internal void AssignToInvoiceItem(Guid invoiceItemId)
    {
        if (invoiceItemId == Guid.Empty)
            throw new DomainException("Invoice item ID cannot be empty.");

        if (InvoiceItemId != Guid.Empty && InvoiceItemId != invoiceItemId)
            throw new DomainException("Invoice add-on already belongs to another invoice item.");

        InvoiceItemId = invoiceItemId;
    }
}
