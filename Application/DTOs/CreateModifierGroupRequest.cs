using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateModifierGroupRequest(
    [Required] string Name,
    [Required] string SelectionType,
    int MinSelection = 0,
    int MaxSelection = 1,
    bool IsRequired = false);
