using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Invoice
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public long InvoiceNumber { get; private set; }
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public Guid IssuedByUserId { get; private set; }
    public DateTime IssuedAtUtc { get; private set; }
    public decimal TaxRate { get; private set; }
    public Money SubTotal { get; private set; } = null!;
    public Money Discount { get; private set; } = null!;
    public Money Tax { get; private set; } = null!;
    public Money GrandTotal { get; private set; } = null!;
    public PaymentStatus PaymentStatus { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }
    public Guid? PaidByUserId { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public Guid? CancelledByUserId { get; private set; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Invoice()
    {
    }

    public static Invoice CreateDraft(Order order, long invoiceNumber, Guid createdByUserId, string currency = "IRR")
    {
        if (order is null)
            throw new DomainException("Invoice requires an order.");

        if (invoiceNumber <= 0)
            throw new DomainException("Invoice number must be positive.");

        if (createdByUserId == Guid.Empty)
            throw new DomainException("Invoice must be created by a valid user.");

        return new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            OrderId = order.Id,
            Order = order,
            IssuedByUserId = createdByUserId,
            IssuedAtUtc = DateTime.UtcNow,
            TaxRate = 0m,
            SubTotal = Money.Zero(currency),
            Discount = Money.Zero(currency),
            Tax = Money.Zero(currency),
            GrandTotal = Money.Zero(currency),
            PaymentStatus = PaymentStatus.Draft
        };
    }

    public void RecalculateFromOrder(Money discount, decimal taxRate)
    {
        if (PaymentStatus != PaymentStatus.Draft)
            throw new DomainException("Invoice can only be recalculated while in draft.");

        if (discount is null)
            throw new DomainException("Invoice discount cannot be null.");

        if (taxRate < 0 || taxRate > 100)
            throw new DomainException("Tax rate must be between 0 and 100.");

        var subTotal = Order.CalculateSubTotal();

        if (discount.Currency != subTotal.Currency)
            throw new DomainException($"Discount currency '{discount.Currency}' does not match order currency '{subTotal.Currency}'.");

        if (discount > subTotal)
            throw new DomainException("Discount cannot be greater than the order sub-total.");

        SubTotal = subTotal;
        Discount = discount;
        TaxRate = taxRate;

        var taxableAmount = subTotal.Subtract(discount);
        Tax = taxableAmount.Percentage(taxRate);
        GrandTotal = taxableAmount.Add(Tax);
    }

    public void MarkPendingPayment()
    {
        if (PaymentStatus != PaymentStatus.Draft)
            throw new DomainException("Invoice must be in draft status to move to pending payment.");

        PaymentStatus = PaymentStatus.PendingPayment;
    }

    public void Pay(Guid paidByUserId)
    {
        if (PaymentStatus != PaymentStatus.PendingPayment)
            throw new DomainException("Only invoices pending payment can be paid.");

        if (paidByUserId == Guid.Empty)
            throw new DomainException("Invoice must be paid by a valid user.");

        PaymentStatus = PaymentStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;
        PaidByUserId = paidByUserId;
        Raise(new InvoicePaid(Id, InvoiceNumber, PaidAtUtc.Value));
    }

    public void Cancel(Guid cancelledByUserId)
    {
        if (PaymentStatus == PaymentStatus.Paid)
            throw new DomainException("Paid invoices cannot be cancelled. Use the refund process instead.");

        if (PaymentStatus == PaymentStatus.Cancelled)
            throw new DomainException("Invoice is already cancelled.");

        if (cancelledByUserId == Guid.Empty)
            throw new DomainException("Invoice must be cancelled by a valid user.");

        PaymentStatus = PaymentStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        CancelledByUserId = cancelledByUserId;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
