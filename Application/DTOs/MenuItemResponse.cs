namespace Application.DTOs;

public record MenuItemResponse(
    Guid Id,
    Guid GroupId,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    int DisplayOrder,
    IReadOnlyList<ModifierGroupResponse>? ModifierGroups = null);
