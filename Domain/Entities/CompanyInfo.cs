using Domain.Exceptions;

namespace Domain.Entities;

public class CompanyInfo
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string LogoUrl { get; private set; } = null!;
    public bool TaxEnabled { get; private set; }
    public decimal TaxRate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime InstallationDate { get; private set; }

    private CompanyInfo()
    {
    }

    private CompanyInfo(string name, string logoUrl, bool taxEnabled, decimal taxRate, DateTime installationDate)
    {
        Id = Guid.NewGuid();
        Name = name;
        LogoUrl = logoUrl;
        TaxEnabled = taxEnabled;
        TaxRate = taxRate;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        InstallationDate = installationDate;
    }

    public static CompanyInfo Create(string name, string logoUrl, bool taxEnabled = false, decimal taxRate = 0m, DateTime? installationDate = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Company name cannot be empty.");

        if (string.IsNullOrWhiteSpace(logoUrl))
            throw new DomainException("Company logo URL cannot be empty.");

        ValidateTaxRate(taxEnabled, taxRate);

        return new CompanyInfo(name.Trim(), logoUrl.Trim(), taxEnabled, taxRate, installationDate ?? DateTime.UtcNow);
    }

    public void UpdateDetails(string name, string logoUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Company name cannot be empty.");

        if (string.IsNullOrWhiteSpace(logoUrl))
            throw new DomainException("Company logo URL cannot be empty.");

        Name = name.Trim();
        LogoUrl = logoUrl.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTaxSettings(bool taxEnabled, decimal taxRate)
    {
        ValidateTaxRate(taxEnabled, taxRate);

        TaxEnabled = taxEnabled;
        TaxRate = taxRate;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateTaxRate(bool taxEnabled, decimal taxRate)
    {
        if (taxEnabled && (taxRate < 0 || taxRate > 100))
            throw new DomainException("Tax rate must be between 0 and 100 when tax is enabled.");
    }
}
