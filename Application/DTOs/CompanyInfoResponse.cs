namespace Application.DTOs;

public record CompanyInfoResponse(
    string Name,
    string LogoUrl,
    bool TaxEnabled,
    decimal TaxRate);

public record UpdateTaxSettingsRequest(
    bool TaxEnabled,
    decimal TaxRate);
