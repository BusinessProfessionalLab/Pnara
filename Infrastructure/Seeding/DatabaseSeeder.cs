using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Auth;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await dbContext.Database.MigrateAsync();

        await SeedRolesAsync(dbContext);
        await SeedDefaultAdminAsync(dbContext, configuration, passwordHasher);
        await SeedCompanyInfoAsync(dbContext);
    }

    private static async Task SeedRolesAsync(AppDbContext dbContext)
    {
        var systemRoles = new[]
        {
            (SystemRoles.Admin, "Full system administrator"),
            (SystemRoles.Operator, "Can manage menu and users"),
            (SystemRoles.User, "Regular customer")
        };

        foreach (var (name, description) in systemRoles)
        {
            if (!await dbContext.Roles.AnyAsync(role => role.Name == name))
            {
                var role = Role.Create(name, description, isSystemRole: true);
                await dbContext.Roles.AddAsync(role);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedDefaultAdminAsync(
        AppDbContext dbContext,
        IConfiguration configuration,
        IPasswordHasher passwordHasher)
    {
        var email = configuration["DefaultAdmin:Email"];
        var password = configuration["DefaultAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (await dbContext.Users.AnyAsync(user => user.Email == normalizedEmail))
            return;

        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(role => role.Name == SystemRoles.Admin);
        if (adminRole is null)
            return;

        var admin = User.CreateByAdmin(
            normalizedEmail,
            passwordHasher.Hash(password),
            configuration["DefaultAdmin:FirstName"] ?? "Admin",
            configuration["DefaultAdmin:LastName"] ?? "System",
            adminRole.Id);

        await dbContext.Users.AddAsync(admin);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedCompanyInfoAsync(AppDbContext dbContext)
    {
        if (await dbContext.CompanyInfos.AnyAsync())
            return;

        var companyInfo = CompanyInfo.Create("Pinara Restaurant", "/uploads/default/logo.png");
        await dbContext.CompanyInfos.AddAsync(companyInfo);
        await dbContext.SaveChangesAsync();
    }
}
