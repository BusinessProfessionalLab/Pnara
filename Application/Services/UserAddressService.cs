using Application.DTOs;
using Application.Exceptions;
using Application.Mappers;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

public class UserAddressService(IUserAddressRepository userAddressRepository)
{
    public async Task<UserAddressResponse> CreateAsync(Guid userId, CreateUserAddressRequest request)
    {
        var address = UserAddress.Create(userId, request.Title, request.AddressLine, request.City, request.PhoneNumber, request.PostalCode, request.IsDefault);

        if (address.IsDefault)
            await ClearOtherDefaultsAsync(userId, excludeAddressId: null);

        await userAddressRepository.AddAsync(address);
        await userAddressRepository.SaveChangesAsync();

        return address.ToResponse();
    }

    public async Task<IReadOnlyList<UserAddressResponse>> GetAllAsync(Guid userId)
    {
        var addresses = await userAddressRepository.GetByUserAsync(userId);
        return addresses.Select(address => address.ToResponse()).ToList();
    }

    public async Task<UserAddressResponse> UpdateAsync(Guid userId, Guid addressId, UpdateUserAddressRequest request)
    {
        var address = await GetOwnedAddressAsync(userId, addressId);

        address.Update(request.Title, request.AddressLine, request.City, request.PhoneNumber, request.PostalCode);

        await userAddressRepository.SaveChangesAsync();
        return address.ToResponse();
    }

    public async Task DeleteAsync(Guid userId, Guid addressId)
    {
        var address = await GetOwnedAddressAsync(userId, addressId);

        userAddressRepository.Remove(address);
        await userAddressRepository.SaveChangesAsync();
    }

    public async Task SetDefaultAsync(Guid userId, Guid addressId)
    {
        var address = await GetOwnedAddressAsync(userId, addressId);

        var addresses = await userAddressRepository.GetByUserAsync(userId);
        foreach (var other in addresses.Where(a => a.Id != addressId && a.IsDefault))
            other.ClearDefault();

        address.SetAsDefault();

        await userAddressRepository.SaveChangesAsync();
    }

    private async Task<UserAddress> GetOwnedAddressAsync(Guid userId, Guid addressId)
    {
        var address = await userAddressRepository.GetByIdAsync(addressId)
            ?? throw new NotFoundException($"Address with id '{addressId}' was not found.");

        if (address.UserId != userId)
            throw new NotFoundException($"Address with id '{addressId}' was not found.");

        return address;
    }

    private async Task ClearOtherDefaultsAsync(Guid userId, Guid? excludeAddressId)
    {
        var addresses = await userAddressRepository.GetByUserAsync(userId);
        foreach (var other in addresses.Where(a => a.IsDefault && a.Id != excludeAddressId))
            other.ClearDefault();
    }
}
