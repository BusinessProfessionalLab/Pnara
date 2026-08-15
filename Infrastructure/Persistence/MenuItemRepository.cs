using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class MenuItemRepository(AppDbContext dbContext) : IMenuItemRepository
{
    public async Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.MenuItems.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MenuItem>> GetByGroupAsync(Guid groupId, CancellationToken cancellationToken = default) =>
        await dbContext.MenuItems
            .Where(item => item.GroupId == groupId)
            .OrderBy(item => item.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(MenuItem item, CancellationToken cancellationToken = default) =>
        await dbContext.MenuItems.AddAsync(item, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
