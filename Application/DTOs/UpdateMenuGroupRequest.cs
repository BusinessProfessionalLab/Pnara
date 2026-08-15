using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record UpdateMenuGroupRequest(
    [Required] string Name,
    int DisplayOrder);
