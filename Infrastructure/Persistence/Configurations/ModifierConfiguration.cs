using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ModifierConfiguration : IEntityTypeConfiguration<Modifier>
{
    public void Configure(EntityTypeBuilder<Modifier> builder)
    {
        builder.ToTable("Modifiers");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(m => m.IsAvailable).IsRequired();
        builder.Property(m => m.DisplayOrder).IsRequired();

        builder.HasIndex(m => m.ModifierGroupId);
    }
}
