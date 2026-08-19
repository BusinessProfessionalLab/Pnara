using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateUserAddressRequest(
    [Required] string Title,
    [Required] string AddressLine,
    string? City,
    [Required] string PhoneNumber,
    string? PostalCode,
    bool IsDefault);

public record UpdateUserAddressRequest(
    [Required] string Title,
    [Required] string AddressLine,
    string? City,
    [Required] string PhoneNumber,
    string? PostalCode);

public record UserAddressResponse(
    Guid Id,
    string Title,
    string AddressLine,
    string? City,
    string PhoneNumber,
    string? PostalCode,
    bool IsDefault);
