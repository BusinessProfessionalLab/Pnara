using Application.DTOs;
using Application.Exceptions;
using Application.Mappers;
using Domain.Repositories;

namespace Application.Services;

public class CompanyInfoService(ICompanyInfoRepository companyInfoRepository)
{
    public async Task<CompanyInfoResponse> GetAsync()
    {
        var companyInfo = await companyInfoRepository.GetAsync()
            ?? throw new NotFoundException("Company information was not found.");

        return companyInfo.ToResponse();
    }
}
