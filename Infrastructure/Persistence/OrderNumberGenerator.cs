using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class OrderNumberGenerator(AppDbContext dbContext) : IOrderNumberGenerator
{
    public async Task<long> NextAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Database
            .SqlQueryRaw<long>("SELECT nextval('\"OrderNumbers\"')")
            .FirstAsync(cancellationToken);
}
