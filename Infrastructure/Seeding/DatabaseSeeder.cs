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

        await SeedPermissionsAsync(dbContext);
        await SeedRolesAsync(dbContext);
        await SeedDefaultAdminAsync(dbContext, configuration, passwordHasher);
        await SeedCompanyInfoAsync(dbContext);
    }

    private static async Task SeedPermissionsAsync(AppDbContext dbContext)
    {
        var catalogNames = SystemPermissions.Catalog.Select(p => p.Name).ToHashSet();

        var stalePermissions = await dbContext.Permissions
            .Where(p => p.IsSystemPermission && !catalogNames.Contains(p.Name))
            .ToListAsync();

        dbContext.Permissions.RemoveRange(stalePermissions);

        var permissionsByName = (await dbContext.Permissions.ToListAsync()).ToDictionary(p => p.Name);

        foreach (var seed in SystemPermissions.Catalog)
        {
            if (permissionsByName.TryGetValue(seed.Name, out var permission))
            {
                permission.UpdateDetails(seed.Description, seed.Group);
            }
            else
            {
                permission = Permission.Create(seed.Name, seed.Description, seed.Group, isSystemPermission: true);
                await dbContext.Permissions.AddAsync(permission);
                permissionsByName[seed.Name] = permission;
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(AppDbContext dbContext)
    {
        var adminRole = await dbContext.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(role => role.Name == SystemRoles.Admin);

        if (adminRole is null)
        {
            adminRole = Role.Create(SystemRoles.Admin, "Full system administrator with all permissions", isSystemRole: true);
            await dbContext.Roles.AddAsync(adminRole);
        }

        foreach (var legacyRoleName in new[] { SystemRoles.Operator, SystemRoles.User })
        {
            var legacyRole = await dbContext.Roles.FirstOrDefaultAsync(role => role.Name == legacyRoleName);

            if (legacyRole is not null && legacyRole.IsSystemRole)
                legacyRole.MarkAsCustomRole();
        }

        var allPermissions = await dbContext.Permissions.ToListAsync();
        foreach (var permission in allPermissions)
        {
            if (adminRole.RolePermissions.All(rp => rp.PermissionId != permission.Id))
                adminRole.AssignPermission(permission);
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
