using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token).HasMaxLength(500).IsRequired();
        builder.HasIndex(rt => rt.Token).IsUnique();

        builder.Property(rt => rt.UserId).IsRequired();

        builder.HasIndex(rt => rt.UserId);
    }
}
