using Domain.Entities;

namespace Domain.Repositories;

public interface ICompanyInfoRepository
{
    Task<CompanyInfo?> GetAsync(CancellationToken cancellationToken = default);
    Task AddAsync(CompanyInfo companyInfo, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
