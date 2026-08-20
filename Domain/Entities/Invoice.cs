using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Invoice
{
    private readonly List<InvoiceItem> _items = [];
    private readonly List<IDomainEvent> _domainEvents = [];

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
    public Guid? OrderId { get; private set; }
    public Order? Order { get; private set; }
    public Guid? IssuedByUserId { get; private set; }
    public decimal TaxRate => Subtotal == 0 ? 0 : TaxAmount / Subtotal * 100m;
    public decimal Discount => DiscountAmount;
    public PaymentStatus PaymentStatus => Status switch
    {
        InvoiceStatus.Finalized => PaymentStatus.Paid,
        InvoiceStatus.Cancelled => PaymentStatus.Cancelled,
        _ => PaymentStatus.Draft
    };
    public DateTime? PaidAtUtc => FinalizedAtUtc;
    public Guid? PaidByUserId { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public PosPaymentState PosPaymentState { get; private set; }
    public string? PaymentReferenceNumber { get; private set; }
    public string? PaymentError { get; private set; }
    public DateTime? PaymentAttemptedAtUtc { get; private set; }
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

    public static Invoice CreateDraft(Order order, long invoiceNumber, Guid issuedByUserId)
    {
        ArgumentNullException.ThrowIfNull(order);
        var invoice = Create(invoiceNumber.ToString(System.Globalization.CultureInfo.InvariantCulture),
            order.Channel == OrderChannel.Web ? SalesChannel.Online : SalesChannel.InPerson);
        invoice.OrderId = order.Id;
        invoice.Order = order;
        invoice.IssuedByUserId = issuedByUserId;
        return invoice;
    }

    public void RecalculateFromOrder(decimal discount, decimal taxRate)
    {
        if (discount < 0 || taxRate < 0 || taxRate > 100)
            throw new DomainException("Invalid discount or tax rate.");
        DiscountAmount = discount;
        TaxAmount = Math.Round(Subtotal * taxRate / 100m, 2);
        RecalculateTotals();
    }

    public void MarkPendingPayment() { }
    public void BeginPosPayment(DateTime? attemptedAtUtc = null)
    {
        if (Status != InvoiceStatus.Draft)
            throw new DomainException("Only draft invoices can start a POS payment.");
        if (TotalAmount <= 0)
            throw new DomainException("Invoice total must be positive.");
        if (PosPaymentState == PosPaymentState.Pending)
            throw new DomainException("A POS payment is already in progress.");

        PosPaymentState = PosPaymentState.Pending;
        PaymentReferenceNumber = null;
        PaymentError = null;
        PaymentAttemptedAtUtc = NormalizeUtc(attemptedAtUtc ?? DateTime.UtcNow);
    }

    public void CompletePosPayment(string referenceNumber)
    {
        if (PosPaymentState != PosPaymentState.Pending)
            throw new DomainException("The invoice has no pending POS payment.");
        if (string.IsNullOrWhiteSpace(referenceNumber))
            throw new DomainException("A successful POS payment requires a reference number.");

        PosPaymentState = PosPaymentState.Succeeded;
        PaymentReferenceNumber = referenceNumber.Trim();
        PaymentError = null;
    }

    public void FailPosPayment(PosPaymentState state, string? error)
    {
        if (state is not (PosPaymentState.Failed or PosPaymentState.Cancelled or PosPaymentState.TimedOut or PosPaymentState.Unknown))
            throw new DomainException("Invalid POS failure state.");
        if (PosPaymentState != PosPaymentState.Pending)
            throw new DomainException("The invoice has no pending POS payment.");

        PosPaymentState = state;
        PaymentError = string.IsNullOrWhiteSpace(error) ? null : error.Trim();
    }

    public void MarkPaymentUnknown(string? error)
    {
        if (Status == InvoiceStatus.Finalized)
            throw new DomainException("Finalized invoices cannot have an unknown payment state.");
        PosPaymentState = PosPaymentState.Unknown;
        PaymentError = string.IsNullOrWhiteSpace(error) ? null : error.Trim();
    }
    public void Pay(Guid userId) { Finalize(Domain.Enums.PaymentMethod.Card); PaidByUserId = userId; }
    public void Cancel(Guid userId) { Cancel(); CancelledByUserId = userId; CancelledAtUtc = DateTime.UtcNow; }
    public void ClearDomainEvents() => _domainEvents.Clear();

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
