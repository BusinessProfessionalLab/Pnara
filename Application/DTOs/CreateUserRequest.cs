using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateUserRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] Guid RoleId);
