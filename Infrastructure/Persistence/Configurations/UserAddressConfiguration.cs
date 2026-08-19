using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresses");
        builder.HasKey(address => address.Id);

        builder.Property(address => address.Title).HasMaxLength(100).IsRequired();
        builder.Property(address => address.AddressLine).HasMaxLength(1000).IsRequired();
        builder.Property(address => address.City).HasMaxLength(100);
        builder.Property(address => address.PhoneNumber).HasMaxLength(30).IsRequired();
        builder.Property(address => address.PostalCode).HasMaxLength(20);

        builder.HasIndex(address => address.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(address => address.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
