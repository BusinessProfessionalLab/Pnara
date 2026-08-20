using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateMenuAddonRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(0, double.MaxValue)] decimal Price,
    int DisplayOrder);

public record UpdateMenuAddonRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(0, double.MaxValue)] decimal Price,
    int DisplayOrder);

public record ReplaceMenuAddonApplicabilityRequest(
    IReadOnlyList<Guid> MenuItemIds);
