using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class MenuGroupRepository(AppDbContext dbContext) : IMenuGroupRepository
{
    public async Task<MenuGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.MenuGroups.FirstOrDefaultAsync(group => group.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MenuGroup>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.MenuGroups
            .OrderBy(group => group.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(MenuGroup group, CancellationToken cancellationToken = default) =>
        await dbContext.MenuGroups.AddAsync(group, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
