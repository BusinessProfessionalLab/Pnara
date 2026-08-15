using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateRoleRequest(
    [Required] string Name,
    string? Description);
