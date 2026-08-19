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

    public async Task<CompanyInfoResponse> UpdateTaxSettingsAsync(UpdateTaxSettingsRequest request)
    {
        var companyInfo = await companyInfoRepository.GetAsync()
            ?? throw new NotFoundException("Company information was not found.");

        companyInfo.UpdateTaxSettings(request.TaxEnabled, request.TaxRate);

        await companyInfoRepository.SaveChangesAsync();
        return companyInfo.ToResponse();
    }
}
