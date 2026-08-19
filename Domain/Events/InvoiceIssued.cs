using Domain.Common;

namespace Domain.Events;

public sealed record InvoiceIssued(
    Guid InvoiceId,
    Guid OrderId,
    long InvoiceNumber,
    decimal GrandTotal,
    string Currency,
    DateTime OccurredAtUtc) : IDomainEvent;
