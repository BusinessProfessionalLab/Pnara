using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateUserRequest(
    [Required] string PhoneNumber,
    [Required][MinLength(8)] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] Guid RoleId);
