using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreatePermissionRequest(
    [Required] string Name,
    string? Description,
    string? Group);
