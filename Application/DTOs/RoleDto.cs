namespace Application.DTOs;

public record RoleDto(Guid Id, string Name, string? Description, bool IsSystemRole, DateTime CreatedAt);
