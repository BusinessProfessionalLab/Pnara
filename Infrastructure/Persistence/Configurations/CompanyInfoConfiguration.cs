using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CompanyInfoConfiguration : IEntityTypeConfiguration<CompanyInfo>
{
    public void Configure(EntityTypeBuilder<CompanyInfo> builder)
    {
        builder.ToTable("CompanyInfos");
        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Name).HasMaxLength(200).IsRequired();
        builder.Property(ci => ci.LogoUrl).HasMaxLength(500).IsRequired();
    }
}
