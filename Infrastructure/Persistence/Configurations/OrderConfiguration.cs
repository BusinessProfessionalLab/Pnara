using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);

        builder.Property(order => order.OrderNumber).IsRequired();
        builder.HasIndex(order => order.OrderNumber).IsUnique();

        builder.Property(order => order.Channel).HasMaxLength(20).IsRequired();
        builder.Property(order => order.Status).HasMaxLength(30).IsRequired();

        builder.Property(order => order.CustomerName).HasMaxLength(200);
        builder.Property(order => order.CustomerPhoneNumber).HasMaxLength(30);
        builder.Property(order => order.DeliveryAddressTitle).HasMaxLength(100);
        builder.Property(order => order.DeliveryAddressLine).HasMaxLength(1000);
        builder.Property(order => order.DeliveryCity).HasMaxLength(100);
        builder.Property(order => order.DeliveryPostalCode).HasMaxLength(20);
        builder.Property(order => order.DeliveryPhoneNumber).HasMaxLength(30);
        builder.Property(order => order.RejectionReason).HasMaxLength(1000);

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(order => order.DomainEvents);

        builder.UseXminConcurrencyToken();
    }
}
