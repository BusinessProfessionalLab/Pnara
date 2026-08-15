using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/company-info")]
public class CompanyInfoController(CompanyInfoService companyInfoService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(CompanyInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompanyInfo() =>
        Ok(await companyInfoService.GetAsync());
}
