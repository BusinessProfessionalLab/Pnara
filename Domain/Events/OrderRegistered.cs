using Domain.Common;
using Domain.Enums;

namespace Domain.Events;

public sealed record OrderRegistered(Guid OrderId, long OrderNumber, OrderChannel Channel, DateTime OccurredAtUtc) : IDomainEvent;
