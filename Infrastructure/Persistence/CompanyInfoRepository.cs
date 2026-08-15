using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class CompanyInfoRepository(AppDbContext dbContext) : ICompanyInfoRepository
{
    public async Task<CompanyInfo?> GetAsync(CancellationToken cancellationToken = default) =>
        await dbContext.CompanyInfos.FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(CompanyInfo companyInfo, CancellationToken cancellationToken = default) =>
        await dbContext.CompanyInfos.AddAsync(companyInfo, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
