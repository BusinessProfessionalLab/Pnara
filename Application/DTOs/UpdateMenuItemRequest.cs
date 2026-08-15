using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record UpdateMenuItemRequest(
    [Required] string Name,
    string? Description,
    [Range(0, double.MaxValue)] decimal Price,
    int DisplayOrder);
