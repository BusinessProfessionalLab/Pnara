using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class ReceiptPrinterMapping
{
    public Guid Id { get; private set; }
    public ReceiptType ReceiptType { get; private set; }
    public Guid PrinterDefinitionId { get; private set; }
    public PrinterDefinition PrinterDefinition { get; private set; } = null!;

    private ReceiptPrinterMapping()
    {
    }

    private ReceiptPrinterMapping(ReceiptType receiptType, Guid printerDefinitionId)
    {
        Id = Guid.NewGuid();
        ReceiptType = receiptType;
        PrinterDefinitionId = printerDefinitionId;
    }

    public static ReceiptPrinterMapping Create(
        ReceiptType receiptType,
        Guid printerDefinitionId)
    {
        if (!Enum.IsDefined(receiptType))
            throw new DomainException("Receipt type is invalid.");

        if (printerDefinitionId == Guid.Empty)
            throw new DomainException("Receipt printer mapping must reference a valid printer.");

        return new ReceiptPrinterMapping(receiptType, printerDefinitionId);
    }

    public void AssignPrinter(Guid printerDefinitionId)
    {
        if (printerDefinitionId == Guid.Empty)
            throw new DomainException("Receipt printer mapping must reference a valid printer.");

        PrinterDefinitionId = printerDefinitionId;
    }
}
