using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class ModifierGroupMapper
{
    public static ModifierGroupResponse ToResponse(this ModifierGroup group) =>
        new(
            group.Id,
            group.Name,
            group.SelectionType.ToString(),
            group.MinSelection,
            group.MaxSelection,
            group.IsRequired,
            group.Modifiers.Select(m => m.ToResponse()).ToList());

    public static ModifierResponse ToResponse(this Modifier modifier) =>
        new(
            modifier.Id,
            modifier.Name,
            modifier.Price,
            modifier.IsAvailable,
            modifier.DisplayOrder);
}
