using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public sealed record ModifierDto(
         Guid Id,
         Guid MenuItemId,
         string Title,
         string? Description,
         bool IsAvailable);
}
