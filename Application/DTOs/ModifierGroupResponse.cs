namespace Application.DTOs;

public record ModifierGroupResponse(
    Guid Id,
    string Name,
    string SelectionType,
    int MinSelection,
    int MaxSelection,
    bool IsRequired,
    IReadOnlyList<ModifierResponse> Modifiers);
