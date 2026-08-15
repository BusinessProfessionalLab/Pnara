using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateMenuGroupRequest(
    [Required] string Name,
    int DisplayOrder);
