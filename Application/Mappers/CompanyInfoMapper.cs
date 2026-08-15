using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class CompanyInfoMapper
{
    public static CompanyInfoResponse ToResponse(this CompanyInfo companyInfo) =>
        new(companyInfo.Name, companyInfo.LogoUrl);
}
