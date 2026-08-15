using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class MenuItemMapper
{
    public static MenuItemResponse ToResponse(this MenuItem item) =>
        new(item.Id, item.GroupId, item.Name, item.Description, item.Price, item.ImageUrl, item.IsAvailable, item.DisplayOrder);
}
