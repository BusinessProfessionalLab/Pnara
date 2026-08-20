using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Xunit;

namespace Domain.Tests;

public class InvoicePosPaymentTests
{
    [Fact]
    public void PosPayment_SuccessStoresReference()
    {
        var invoice = Invoice.Create("INV-1", SalesChannel.InPerson);
        invoice.AddItem(InvoiceItem.Create(Guid.NewGuid(), "Coffee", 1, 100));
        invoice.BeginPosPayment();
        invoice.CompletePosPayment("REF-123");

        Assert.Equal(PosPaymentState.Succeeded, invoice.PosPaymentState);
        Assert.Equal("REF-123", invoice.PaymentReferenceNumber);
    }

    [Fact]
    public void PosPayment_CannotCompleteWithoutPendingAttempt()
    {
        var invoice = Invoice.Create("INV-1", SalesChannel.InPerson);
        Assert.Throws<DomainException>(() => invoice.CompletePosPayment("REF-123"));
    }
}
