using Domain.Exceptions;

namespace Domain.Entities;

public class CompanyInfo
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string LogoUrl { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private CompanyInfo()
    {
    }

    private CompanyInfo(string name, string logoUrl)
    {
        Id = Guid.NewGuid();
        Name = name;
        LogoUrl = logoUrl;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static CompanyInfo Create(string name, string logoUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Company name cannot be empty.");

        if (string.IsNullOrWhiteSpace(logoUrl))
            throw new DomainException("Company logo URL cannot be empty.");

        return new CompanyInfo(name.Trim(), logoUrl.Trim());
    }
}
