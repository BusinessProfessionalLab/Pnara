using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class PermissionRepository(AppDbContext dbContext) : IPermissionRepository
{
    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Permissions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Permissions.OrderBy(p => p.Name).ToListAsync(cancellationToken);

    public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

    public async Task AddAsync(Permission permission, CancellationToken cancellationToken = default) =>
        await dbContext.Permissions.AddAsync(permission, cancellationToken);

    public async Task<IReadOnlyList<Permission>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        await dbContext.Permissions.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);

    public Task Remove(Permission permission, CancellationToken cancellationToken = default)
    {
        dbContext.Permissions.Remove(permission);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await dbContext.Permissions.AnyAsync(p => p.Name == name, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
