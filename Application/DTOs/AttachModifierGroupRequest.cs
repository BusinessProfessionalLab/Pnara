using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record AttachModifierGroupRequest([Required] Guid ModifierGroupId);
