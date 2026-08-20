namespace Domain.Enums;

public enum PaymentStatus
{
    Draft,
    PendingPayment,
    Paid,
    Cancelled,
    Pending = PendingPayment
}
