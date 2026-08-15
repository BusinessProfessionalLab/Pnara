namespace Application.DTOs;

public record PublicMenuResponse(IReadOnlyList<PublicMenuGroupDto> Groups);

public record PublicMenuGroupDto(
    Guid Id,
    string Name,
    int DisplayOrder,
    IReadOnlyList<PublicMenuItemDto> Items);

public record PublicMenuItemDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    int DisplayOrder);
