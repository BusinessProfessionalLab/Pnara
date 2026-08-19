using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class UserAddressMapper
{
    public static UserAddressResponse ToResponse(this UserAddress address) =>
        new(address.Id, address.Title, address.AddressLine, address.City, address.PhoneNumber, address.PostalCode, address.IsDefault);
}
