using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class InvoiceNumberGenerator(AppDbContext dbContext) : IInvoiceNumberGenerator
{
    public async Task<long> NextAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Database
            .SqlQueryRaw<long>("SELECT nextval('\"InvoiceNumbers\"') AS \"Value\"")
            .FirstAsync(cancellationToken);
}
