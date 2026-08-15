using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateMenuItemRequest(
    [Required] Guid GroupId,
    [Required] string Name,
    string? Description,
    [Range(0, double.MaxValue)] decimal Price,
    string? ImageUrl,
    int DisplayOrder);
