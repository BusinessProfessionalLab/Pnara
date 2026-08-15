using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record UpdateRoleRequest(
    [Required] string Name,
    string? Description);
