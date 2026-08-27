using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record UpdateUserRequest(
    string? PhoneNumber,
    [MinLength(8)] string? Password,
    string? FirstName,
    string? LastName);
