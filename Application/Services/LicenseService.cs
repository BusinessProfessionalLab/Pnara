namespace Application.Services;

using Application.Common;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Repositories;

public class LicenseService : ILicenseService
{
    private readonly ICompanyInfoRepository _companyInfoRepository;
    private readonly LicenseSettings _licenseSettings;

    public LicenseService(ICompanyInfoRepository companyInfoRepository, LicenseSettings licenseSettings)
    {
        _companyInfoRepository = companyInfoRepository;
        _licenseSettings = licenseSettings;
    }

    public async Task ValidateTrialAsync()
    {
        if (_licenseSettings.TrialDays <= 0)
            return;

        var companyInfo = await _companyInfoRepository.GetAsync();
        if (companyInfo is null)
            return;

        var expirationDate = companyInfo.InstallationDate.AddDays(_licenseSettings.TrialDays);
        if (DateTime.UtcNow > expirationDate)
            throw new TrialExpiredException();
    }
}

