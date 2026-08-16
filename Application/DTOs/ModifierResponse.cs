namespace Application.DTOs;

public record ModifierResponse(
    Guid Id,
    string Name,
    decimal Price,
    bool IsAvailable,
    int DisplayOrder);
