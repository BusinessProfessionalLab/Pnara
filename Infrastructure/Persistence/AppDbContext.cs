using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<MenuGroup> MenuGroups => Set<MenuGroup>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<CompanyInfo> CompanyInfos => Set<CompanyInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompanyInfo>(entity =>
        {
            entity.ToTable("CompanyInfos");
            entity.HasKey(ci => ci.Id);

            entity.Property(ci => ci.Name).HasMaxLength(200).IsRequired();
            entity.Property(ci => ci.LogoUrl).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(role => role.Id);

            entity.Property(role => role.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(role => role.Name).IsUnique();

            entity.Property(role => role.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();

            entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.LastName).HasMaxLength(100).IsRequired();

            entity.HasOne(user => user.Role)
                .WithMany()
                .HasForeignKey(user => user.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Navigation(user => user.Role).AutoInclude();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(rt => rt.Id);

            entity.Property(rt => rt.Token).HasMaxLength(500).IsRequired();
            entity.HasIndex(rt => rt.Token).IsUnique();

            entity.Property(rt => rt.UserId).IsRequired();

            entity.HasIndex(rt => rt.UserId);
        });

        modelBuilder.Entity<MenuGroup>(entity =>
        {
            entity.ToTable("MenuGroups");
            entity.HasKey(group => group.Id);

            entity.Property(group => group.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.ToTable("MenuItems");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(1000);
            entity.Property(item => item.ImageUrl).HasMaxLength(500);
            entity.Property(item => item.Price).HasPrecision(18, 2).IsRequired();

            entity.HasIndex(item => item.GroupId);
        });
    }
}
