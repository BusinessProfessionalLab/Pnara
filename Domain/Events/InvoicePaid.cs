using Domain.Common;

namespace Domain.Events;

public sealed record InvoicePaid(Guid InvoiceId, long InvoiceNumber, DateTime OccurredAtUtc) : IDomainEvent;
