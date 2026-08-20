using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class PosTerminalRepository(AppDbContext dbContext) : IPosTerminalRepository
{
    public Task<PosTerminalDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.PosTerminalDefinitions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<PosTerminalDefinition?> GetActiveAsync(CancellationToken cancellationToken = default) =>
        dbContext.PosTerminalDefinitions
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<PosTerminalDefinition>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.PosTerminalDefinitions.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task AddAsync(PosTerminalDefinition terminal, CancellationToken cancellationToken = default) =>
        dbContext.PosTerminalDefinitions.AddAsync(terminal, cancellationToken).AsTask();

    public void Remove(PosTerminalDefinition terminal) => dbContext.PosTerminalDefinitions.Remove(terminal);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
