namespace Application.DTOs;

public record MenuGroupResponse(
    Guid Id,
    string Name,
    int DisplayOrder,
    bool IsActive);
