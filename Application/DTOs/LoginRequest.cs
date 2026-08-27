using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record LoginRequest(
    [Required] string PhoneNumber,
    [Required] string Password);
