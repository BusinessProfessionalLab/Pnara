using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ModifierGroupConfiguration : IEntityTypeConfiguration<ModifierGroup>
{
    public void Configure(EntityTypeBuilder<ModifierGroup> builder)
    {
        builder.ToTable("ModifierGroups");
        builder.HasKey(mg => mg.Id);

        builder.Property(mg => mg.Name).HasMaxLength(200).IsRequired();
        builder.Property(mg => mg.SelectionType).HasMaxLength(50).IsRequired();
        builder.Property(mg => mg.MinSelection).IsRequired();
        builder.Property(mg => mg.MaxSelection).IsRequired();
        builder.Property(mg => mg.IsRequired).IsRequired();

        builder.HasMany(mg => mg.Modifiers)
            .WithOne()
            .HasForeignKey(m => m.ModifierGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(mg => mg.MenuItems);
    }
}
