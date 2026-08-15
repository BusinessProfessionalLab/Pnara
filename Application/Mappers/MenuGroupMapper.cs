using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class MenuGroupMapper
{
    public static MenuGroupResponse ToResponse(this MenuGroup group) =>
        new(group.Id, group.Name, group.DisplayOrder, group.IsActive);
}
