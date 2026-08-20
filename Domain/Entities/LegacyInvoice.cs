using Domain.Enums;
using Domain.Exceptions;
using Domain.Events;
using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class LegacyInvoice
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public Guid Id { get; } = Guid.NewGuid();
    public Guid OrderId { get; }
    public long InvoiceNumber { get; }
    public Money SubTotal { get; }
    public Money Discount { get; }
    public Money Tax { get; }
    public Money GrandTotal { get; }
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.PendingPayment;
    public DateTime? PaidAtUtc { get; private set; }
    public Guid? PaidByUserId { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private LegacyInvoice(long number, Money subtotal, Money discount, decimal taxRate, Guid orderId)
    {
        if (number <= 0) throw new DomainException("Invoice number must be positive.");
        if (discount > subtotal) throw new DomainException("Discount cannot exceed subtotal.");
        if (taxRate < 0 || taxRate > 100) throw new DomainException("Tax rate must be between 0 and 100.");
        InvoiceNumber = number;
        OrderId = orderId;
        SubTotal = subtotal;
        Discount = discount;
        Tax = subtotal.Subtract(discount).Percentage(taxRate);
        GrandTotal = subtotal.Subtract(discount).Add(Tax);
    }

    internal static LegacyInvoice Create(Order order, long number, Money discount, decimal taxRate) =>
        new(number, order.CalculateSubTotal(), discount, taxRate, order.Id);

    public void Pay(Guid userId)
    {
        if (PaymentStatus != PaymentStatus.PendingPayment) throw new DomainException("Only pending invoices can be paid.");
        PaymentStatus = PaymentStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;
        PaidByUserId = userId;
        _domainEvents.Add(new InvoicePaid(Id, InvoiceNumber, PaidAtUtc.Value));
    }

    public void Cancel(Guid userId)
    {
        if (PaymentStatus != PaymentStatus.PendingPayment) throw new DomainException("Only pending invoices can be cancelled.");
        PaymentStatus = PaymentStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        CancelledByUserId = userId;
    }
}
