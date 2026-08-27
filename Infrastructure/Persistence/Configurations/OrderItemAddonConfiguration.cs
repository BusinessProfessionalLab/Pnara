using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderItemAddonConfiguration : IEntityTypeConfiguration<OrderItemAddon>
{
    public void Configure(EntityTypeBuilder<OrderItemAddon> builder)
    {
        builder.ToTable("OrderItemAddons");
        builder.HasKey(addon => addon.Id);

        builder.Property(addon => addon.AddonName).HasMaxLength(200).IsRequired();
        builder.Property(addon => addon.Quantity).HasPrecision(18, 3).IsRequired();
        builder.Property(addon => addon.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(addon => addon.LineTotal).HasPrecision(18, 2).IsRequired();

        builder.HasIndex(addon => addon.ModifierId);
        builder.HasIndex(addon => addon.OrderItemId);
    }
}
