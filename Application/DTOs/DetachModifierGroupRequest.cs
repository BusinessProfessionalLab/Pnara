using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record DetachModifierGroupRequest([Required] Guid ModifierGroupId);
