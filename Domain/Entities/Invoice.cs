using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Invoice
{
    private readonly List<InvoiceItem> _items = [];

    public Guid Id { get; private set; }
    public string InvoiceNumber { get; private set; } = null!;
    public SalesChannel Channel { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime IssuedAtUtc { get; private set; }
    public DateTime? FinalizedAtUtc { get; private set; }
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Invoice()
    {
    }

    private Invoice(
        string invoiceNumber,
        SalesChannel channel,
        decimal discountAmount,
        decimal taxAmount,
        DateTime issuedAtUtc)
    {
        Id = Guid.NewGuid();
        InvoiceNumber = invoiceNumber;
        Channel = channel;
        Status = InvoiceStatus.Draft;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        IssuedAtUtc = issuedAtUtc;
    }

    public static Invoice Create(
        string invoiceNumber,
        SalesChannel channel,
        decimal discountAmount = 0,
        decimal taxAmount = 0,
        DateTime? issuedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new DomainException("Invoice number cannot be empty.");

        if (discountAmount < 0)
            throw new DomainException("Invoice discount cannot be negative.");

        if (taxAmount < 0)
            throw new DomainException("Invoice tax cannot be negative.");

        if (!Enum.IsDefined(channel))
            throw new DomainException("Invoice channel is invalid.");

        return new Invoice(
            invoiceNumber.Trim(),
            channel,
            discountAmount,
            taxAmount,
            NormalizeUtc(issuedAtUtc ?? DateTime.UtcNow));
    }

    public void AddItem(InvoiceItem item)
    {
        if (Status != InvoiceStatus.Draft)
            throw new DomainException("Only draft invoices can be changed.");

        ArgumentNullException.ThrowIfNull(item);
        item.AssignToInvoice(Id);
        _items.Add(item);
        RecalculateTotals();
    }

    public void Finalize(PaymentMethod paymentMethod, DateTime? finalizedAtUtc = null)
    {
        if (Status != InvoiceStatus.Draft)
            throw new DomainException("Only draft invoices can be finalized.");

        if (_items.Count == 0)
            throw new DomainException("An invoice must contain at least one item.");

        if (DiscountAmount > Subtotal)
            throw new DomainException("Invoice discount cannot exceed the subtotal.");

        if (!Enum.IsDefined(paymentMethod))
            throw new DomainException("Invoice payment method is invalid.");

        PaymentMethod = paymentMethod;
        Status = InvoiceStatus.Finalized;
        FinalizedAtUtc = NormalizeUtc(finalizedAtUtc ?? DateTime.UtcNow);
        RecalculateTotals();
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Finalized)
            throw new DomainException("Finalized invoices cannot be cancelled.");

        Status = InvoiceStatus.Cancelled;
    }

    private void RecalculateTotals()
    {
        Subtotal = _items.Sum(item => item.LineTotal);
        TotalAmount = Subtotal - DiscountAmount + TaxAmount;
    }

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
