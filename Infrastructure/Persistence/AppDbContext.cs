using Domain.Entities;
using Infrastructure.Persistence.Configurations;
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
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<InvoiceItemAddon> InvoiceItemAddons => Set<InvoiceItemAddon>();
    public DbSet<MenuAddon> MenuAddons => Set<MenuAddon>();
    public DbSet<MenuAddonMenuItem> MenuAddonMenuItems => Set<MenuAddonMenuItem>();
    public DbSet<MenuAddonRecipe> MenuAddonRecipes => Set<MenuAddonRecipe>();
    public DbSet<MenuAddonRecipeComponent> MenuAddonRecipeComponents => Set<MenuAddonRecipeComponent>();
    public DbSet<PrinterDefinition> PrinterDefinitions => Set<PrinterDefinition>();
    public DbSet<ReceiptTemplate> ReceiptTemplates => Set<ReceiptTemplate>();
    public DbSet<ReceiptPrinterMapping> ReceiptPrinterMappings => Set<ReceiptPrinterMapping>();
    public DbSet<MeasurementUnit> MeasurementUnits => Set<MeasurementUnit>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<StockLedgerEntry> StockLedgerEntries => Set<StockLedgerEntry>();
    public DbSet<MenuItemRecipe> MenuItemRecipes => Set<MenuItemRecipe>();
    public DbSet<RecipeComponent> RecipeComponents => Set<RecipeComponent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("OrderNumbers").StartsAt(1);
        modelBuilder.HasSequence<long>("InvoiceNumbers").StartsAt(1);

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

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(invoice => invoice.Id);

            entity.Property(invoice => invoice.InvoiceNumber)
                .HasMaxLength(50)
                .IsRequired();
            entity.HasIndex(invoice => invoice.InvoiceNumber)
                .IsUnique();

            entity.Property(invoice => invoice.Channel)
                .HasConversion<int>()
                .IsRequired();
            entity.Property(invoice => invoice.Status)
                .HasConversion<int>()
                .IsRequired();
            entity.Property(invoice => invoice.PaymentMethod)
                .HasConversion<int?>();

            entity.Property(invoice => invoice.Subtotal)
                .HasPrecision(18, 2)
                .IsRequired();
            entity.Property(invoice => invoice.DiscountAmount)
                .HasPrecision(18, 2)
                .IsRequired();
            entity.Property(invoice => invoice.TaxAmount)
                .HasPrecision(18, 2)
                .IsRequired();
            entity.Property(invoice => invoice.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();
            entity.Property(invoice => invoice.IssuedAtUtc)
                .IsRequired();
            entity.Property(invoice => invoice.FinalizedAtUtc);

            entity.HasIndex(invoice => new { invoice.Status, invoice.FinalizedAtUtc });
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.ToTable("InvoiceItems");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.ItemName)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(item => item.Quantity)
                .HasPrecision(18, 3)
                .IsRequired();
            entity.Property(item => item.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();
            entity.Property(item => item.LineTotal)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.HasIndex(item => item.MenuItemId);
            entity.HasOne<Invoice>()
                .WithMany(invoice => invoice.Items)
                .HasForeignKey(item => item.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceItemAddon>(entity =>
        {
            entity.ToTable("InvoiceItemAddons");
            entity.HasKey(addon => addon.Id);

            entity.Property(addon => addon.AddonName)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(addon => addon.Quantity)
                .HasPrecision(18, 3)
                .IsRequired();
            entity.Property(addon => addon.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();
            entity.Property(addon => addon.LineTotal)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.HasIndex(addon => addon.MenuAddonId);
            entity.HasOne<InvoiceItem>()
                .WithMany(item => item.Addons)
                .HasForeignKey(addon => addon.InvoiceItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<MenuAddon>()
                .WithMany()
                .HasForeignKey(addon => addon.MenuAddonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MenuAddon>(entity =>
        {
            entity.ToTable("MenuAddons");
            entity.HasKey(addon => addon.Id);

            entity.Property(addon => addon.Name)
                .HasMaxLength(200)
                .IsRequired();
            entity.HasIndex(addon => addon.Name)
                .IsUnique();
            entity.Property(addon => addon.Description)
                .HasMaxLength(1000);
            entity.Property(addon => addon.Price)
                .HasPrecision(18, 2)
                .IsRequired();
        });

        modelBuilder.Entity<MenuAddonMenuItem>(entity =>
        {
            entity.ToTable("MenuAddonMenuItems");
            entity.HasKey(applicability => new
            {
                applicability.MenuAddonId,
                applicability.MenuItemId
            });

            entity.HasIndex(applicability => applicability.MenuItemId);
            entity.HasOne<MenuAddon>()
                .WithMany()
                .HasForeignKey(applicability => applicability.MenuAddonId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<MenuItem>()
                .WithMany()
                .HasForeignKey(applicability => applicability.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MeasurementUnit>(entity =>
        {
            entity.ToTable("MeasurementUnits");
            entity.HasKey(unit => unit.Id);

            entity.Property(unit => unit.Name)
                .HasMaxLength(100)
                .IsRequired();
            entity.HasIndex(unit => unit.Name)
                .IsUnique();

            entity.Property(unit => unit.Symbol)
                .HasMaxLength(20)
                .IsRequired();
            entity.HasIndex(unit => unit.Symbol)
                .IsUnique();
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.ToTable("Ingredients");
            entity.HasKey(ingredient => ingredient.Id);

            entity.Property(ingredient => ingredient.Name)
                .HasMaxLength(200)
                .IsRequired();
            entity.HasIndex(ingredient => ingredient.Name)
                .IsUnique();

            entity.Property(ingredient => ingredient.CurrentStock)
                .HasPrecision(18, 3)
                .IsRequired();
            entity.Property(ingredient => ingredient.MinimumStock)
                .HasPrecision(18, 3)
                .IsRequired();
            entity.Property(ingredient => ingredient.ConcurrencyToken)
                .IsConcurrencyToken()
                .IsRequired();
            entity.Ignore(ingredient => ingredient.IsLowStock);

            entity.HasIndex(ingredient => ingredient.MeasurementUnitId);
            entity.HasOne<MeasurementUnit>()
                .WithMany()
                .HasForeignKey(ingredient => ingredient.MeasurementUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockLedgerEntry>(entity =>
        {
            entity.ToTable("StockLedgerEntries");
            entity.HasKey(entry => entry.Id);

            entity.Property(entry => entry.MovementType)
                .HasConversion<int>()
                .IsRequired();
            entity.Property(entry => entry.QuantityChange)
                .HasPrecision(18, 3)
                .IsRequired();
            entity.Property(entry => entry.BalanceAfter)
                .HasPrecision(18, 3)
                .IsRequired();
            entity.Property(entry => entry.Note)
                .HasMaxLength(500);
            entity.Property(entry => entry.OccurredAtUtc)
                .IsRequired();

            entity.HasIndex(entry => new { entry.IngredientId, entry.OccurredAtUtc });
            entity.HasIndex(entry => new { entry.InvoiceId, entry.IngredientId, entry.MovementType })
                .IsUnique()
                .HasFilter("\"InvoiceId\" IS NOT NULL");

            entity.HasOne<Ingredient>()
                .WithMany()
                .HasForeignKey(entry => entry.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Invoice>()
                .WithMany()
                .HasForeignKey(entry => entry.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MenuItemRecipe>(entity =>
        {
            entity.ToTable("MenuItemRecipes");
            entity.HasKey(recipe => recipe.Id);

            entity.HasIndex(recipe => recipe.MenuItemId)
                .IsUnique();
            entity.HasOne<MenuItem>()
                .WithOne()
                .HasForeignKey<MenuItemRecipe>(recipe => recipe.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeComponent>(entity =>
        {
            entity.ToTable("RecipeComponents");
            entity.HasKey(component => component.Id);

            entity.Property(component => component.Quantity)
                .HasPrecision(18, 3)
                .IsRequired();

            entity.HasIndex(component => new { component.RecipeId, component.IngredientId })
                .IsUnique();
            entity.HasOne<MenuItemRecipe>()
                .WithMany(recipe => recipe.Components)
                .HasForeignKey(component => component.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Ingredient>()
                .WithMany()
                .HasForeignKey(component => component.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MenuAddonRecipe>(entity =>
        {
            entity.ToTable("MenuAddonRecipes");
            entity.HasKey(recipe => recipe.Id);

            entity.HasIndex(recipe => recipe.MenuAddonId)
                .IsUnique();
            entity.HasOne<MenuAddon>()
                .WithOne()
                .HasForeignKey<MenuAddonRecipe>(recipe => recipe.MenuAddonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MenuAddonRecipeComponent>(entity =>
        {
            entity.ToTable("MenuAddonRecipeComponents");
            entity.HasKey(component => component.Id);

            entity.Property(component => component.Quantity)
                .HasPrecision(18, 3)
                .IsRequired();

            entity.HasIndex(component => new
            {
                component.RecipeId,
                component.IngredientId
            }).IsUnique();
            entity.HasOne<MenuAddonRecipe>()
                .WithMany(recipe => recipe.Components)
                .HasForeignKey(component => component.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Ingredient>()
                .WithMany()
                .HasForeignKey(component => component.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PrinterDefinition>(entity =>
        {
            entity.ToTable("PrinterDefinitions");
            entity.HasKey(printer => printer.Id);

            entity.Property(printer => printer.Name)
                .HasMaxLength(200)
                .IsRequired();
            entity.HasIndex(printer => printer.Name)
                .IsUnique();
            entity.Property(printer => printer.ConnectionType)
                .HasConversion<int>()
                .IsRequired();
            entity.Property(printer => printer.Host)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(printer => printer.Port)
                .IsRequired();
            entity.Property(printer => printer.PaperWidth)
                .IsRequired();
        });

        modelBuilder.Entity<ReceiptTemplate>(entity =>
        {
            entity.ToTable("ReceiptTemplates");
            entity.HasKey(template => template.Id);

            entity.Property(template => template.ReceiptType)
                .HasConversion<int>()
                .IsRequired();
            entity.HasIndex(template => template.ReceiptType)
                .IsUnique();
            entity.Property(template => template.HeaderText)
                .HasMaxLength(1000);
            entity.Property(template => template.FooterText)
                .HasMaxLength(1000);
            entity.Property(template => template.FontSize)
                .IsRequired();
        });

        modelBuilder.Entity<ReceiptPrinterMapping>(entity =>
        {
            entity.ToTable("ReceiptPrinterMappings");
            entity.HasKey(mapping => mapping.Id);

            entity.Property(mapping => mapping.ReceiptType)
                .HasConversion<int>()
                .IsRequired();
            entity.HasIndex(mapping => mapping.ReceiptType)
                .IsUnique();
            entity.HasOne(mapping => mapping.PrinterDefinition)
                .WithMany()
                .HasForeignKey(mapping => mapping.PrinterDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
