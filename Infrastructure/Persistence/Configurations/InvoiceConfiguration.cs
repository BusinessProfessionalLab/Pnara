using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.InvoiceNumber).IsRequired();
        builder.HasIndex(invoice => invoice.InvoiceNumber).IsUnique();

        builder.Property(invoice => invoice.TaxRate).HasPrecision(5, 2).IsRequired();
        builder.Property(invoice => invoice.PaymentStatus).HasMaxLength(20).IsRequired();

        builder.OwnsOne(invoice => invoice.SubTotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("SubTotal").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("SubTotalCurrency").HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(invoice => invoice.Discount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Discount").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("DiscountCurrency").HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(invoice => invoice.Tax, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Tax").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("TaxCurrency").HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(invoice => invoice.GrandTotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("GrandTotal").HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasColumnName("GrandTotalCurrency").HasMaxLength(3).IsRequired();
        });

        builder.HasOne(invoice => invoice.Order)
            .WithMany()
            .HasForeignKey(invoice => invoice.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(invoice => invoice.DomainEvents);

        builder.UseXminConcurrencyToken();
    }
}
