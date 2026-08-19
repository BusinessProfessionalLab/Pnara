using Application.DTOs;
using Application.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/company-info")]
public class CompanyInfoController(CompanyInfoService companyInfoService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(CompanyInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompanyInfo() =>
        Ok(await companyInfoService.GetAsync());

    [Authorize(Policy = "perm:settings.manage")]
    [HttpPatch("tax-settings")]
    [ProducesResponseType(typeof(CompanyInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTaxSettings(UpdateTaxSettingsRequest request) =>
        Ok(await companyInfoService.UpdateTaxSettingsAsync(request));
}
