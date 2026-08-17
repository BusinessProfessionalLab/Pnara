using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class RoleRepository(AppDbContext dbContext) : IRoleRepository
{
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Roles.FirstOrDefaultAsync(role => role.Id == id, cancellationToken);

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await dbContext.Roles.FirstOrDefaultAsync(role => role.Name == name, cancellationToken);

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Roles.OrderBy(role => role.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default) =>
        await dbContext.Roles.AddAsync(role, cancellationToken);

    public Task Update(Role role, CancellationToken cancellationToken = default)
    {
        dbContext.Roles.Update(role);
        return Task.CompletedTask;
    }

    public Task Delete(Role role, CancellationToken cancellationToken = default)
    {
        dbContext.Roles.Remove(role);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await dbContext.Roles.AnyAsync(role => role.Name == name, cancellationToken);

    public async Task<bool> HasUsersAsync(Guid roleId, CancellationToken cancellationToken = default) =>
        await dbContext.Users.AnyAsync(user => user.RoleId == roleId, cancellationToken);

    public async Task<Role?> GetWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
