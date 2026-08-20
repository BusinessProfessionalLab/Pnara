using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class StockLedgerEntry
{
    public Guid Id { get; private set; }
    public Guid IngredientId { get; private set; }
    public StockMovementType MovementType { get; private set; }
    public decimal QuantityChange { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public string? Note { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private StockLedgerEntry()
    {
    }

    private StockLedgerEntry(
        Guid ingredientId,
        StockMovementType movementType,
        decimal quantityChange,
        decimal balanceAfter,
        Guid? invoiceId,
        string? note,
        DateTime occurredAtUtc)
    {
        Id = Guid.NewGuid();
        IngredientId = ingredientId;
        MovementType = movementType;
        QuantityChange = quantityChange;
        BalanceAfter = balanceAfter;
        InvoiceId = invoiceId;
        Note = note;
        OccurredAtUtc = occurredAtUtc;
    }

    public static StockLedgerEntry Create(
        Guid ingredientId,
        StockMovementType movementType,
        decimal quantityChange,
        decimal balanceAfter,
        Guid? invoiceId = null,
        string? note = null,
        DateTime? occurredAtUtc = null)
    {
        if (ingredientId == Guid.Empty)
            throw new DomainException("Stock ledger entry must reference a valid ingredient.");

        if (!Enum.IsDefined(movementType))
            throw new DomainException("Stock movement type is invalid.");

        if (quantityChange == 0)
            throw new DomainException("Stock ledger quantity change cannot be zero.");

        if (balanceAfter < 0)
            throw new DomainException("Stock ledger balance cannot be negative.");

        if (movementType == StockMovementType.InvoiceConsumption && invoiceId is null)
            throw new DomainException("Invoice consumption must reference an invoice.");

        if (movementType != StockMovementType.InvoiceConsumption && invoiceId is not null)
            throw new DomainException("Only invoice consumption can reference an invoice.");

        return new StockLedgerEntry(
            ingredientId,
            movementType,
            quantityChange,
            balanceAfter,
            invoiceId,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            NormalizeUtc(occurredAtUtc ?? DateTime.UtcNow));
    }

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
