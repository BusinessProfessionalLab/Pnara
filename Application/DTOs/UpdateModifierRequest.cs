using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record UpdateModifierRequest(
    [Required] string Name,
    [Range(0, double.MaxValue)] decimal Price = 0,
    int DisplayOrder = 0);
