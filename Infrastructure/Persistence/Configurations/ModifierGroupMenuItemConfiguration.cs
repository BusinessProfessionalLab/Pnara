using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ModifierGroupMenuItemConfiguration : IEntityTypeConfiguration<ModifierGroupMenuItem>
{
    public void Configure(EntityTypeBuilder<ModifierGroupMenuItem> builder)
    {
        builder.ToTable("ModifierGroupMenuItems");
        builder.HasKey(x => new { x.ModifierGroupId, x.MenuItemId });

        builder.HasOne<ModifierGroup>()
            .WithMany()
            .HasForeignKey(x => x.ModifierGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<MenuItem>()
            .WithMany()
            .HasForeignKey(x => x.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
