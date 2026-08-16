using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappers;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;

namespace Application.Services;

public class MenuItemService(IMenuItemRepository menuItemRepository, IFileStorage fileStorage, IModifierGroupRepository modifierGroupRepository)
{
    public async Task<MenuItemResponse> CreateAsync(CreateMenuItemRequest request)
    {
        var item = MenuItem.Create(request.GroupId, request.Name, request.Description, request.Price, request.ImageUrl, request.DisplayOrder);

        await menuItemRepository.AddAsync(item);
        await menuItemRepository.SaveChangesAsync();

        return item.ToResponse();
    }

    public async Task<MenuItemResponse> CreateWithImageAsync(CreateMenuItemRequest request, Stream file, string fileName, string contentType, long fileSize)
    {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(contentType))
            throw new DomainException("Only JPG, PNG and WebP images are allowed.");

        const long maxSize = 2 * 1024 * 1024;
        if (fileSize > maxSize)
            throw new DomainException("File size cannot exceed 2MB.");

        var imageUrl = await fileStorage.SaveAsync(file, "menu-items", fileName);

        var item = MenuItem.Create(request.GroupId, request.Name, request.Description, request.Price, imageUrl, request.DisplayOrder);

        await menuItemRepository.AddAsync(item);
        await menuItemRepository.SaveChangesAsync();

        return item.ToResponse();
    }

    public async Task<MenuItemResponse> UpdateAsync(Guid id, UpdateMenuItemRequest request)
    {
        var item = await menuItemRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Menu item with id '{id}' was not found.");

        item.Update(request.Name, request.Description, request.Price, request.DisplayOrder);

        await menuItemRepository.SaveChangesAsync();

        return item.ToResponse();
    }

    public async Task ActivateAsync(Guid id)
    {
        var item = await menuItemRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Menu item with id '{id}' was not found.");

        item.Activate();

        await menuItemRepository.SaveChangesAsync();
    }

    public async Task DeactivateAsync(Guid id)
    {
        var item = await menuItemRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Menu item with id '{id}' was not found.");

        item.Deactivate();

        await menuItemRepository.SaveChangesAsync();
    }

    public async Task SetStatusAsync(Guid id, bool isAvailable)
    {
        var item = await menuItemRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Menu item with id '{id}' was not found.");

        if (isAvailable)
            item.Activate();
        else
            item.Deactivate();

        await menuItemRepository.SaveChangesAsync();
    }

    public async Task ChangeAvailabilityAsync(Guid id, ChangeAvailabilityRequest request)
    {
        var item = await menuItemRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Menu item with id '{id}' was not found.");

        item.ChangeAvailability(request.IsAvailable);

        await menuItemRepository.SaveChangesAsync();
    }

    public async Task RemoveImageAsync(Guid id)
    {
        var item = await menuItemRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Menu item with id '{id}' was not found.");

        if (!string.IsNullOrEmpty(item.ImageUrl))
            await fileStorage.DeleteAsync(item.ImageUrl);

        item.RemoveImage();

        await menuItemRepository.SaveChangesAsync();
    }

    public async Task<MenuItemResponse> UploadImageAsync(Guid id, Stream file, string fileName, string contentType, long fileSize)
    {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(contentType))
            throw new DomainException("Only JPG, PNG and WebP images are allowed.");

        const long maxSize = 2 * 1024 * 1024;
        if (fileSize > maxSize)
            throw new DomainException("File size cannot exceed 2MB.");

        var item = await menuItemRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Menu item with id '{id}' was not found.");

        if (!string.IsNullOrEmpty(item.ImageUrl))
            await fileStorage.DeleteAsync(item.ImageUrl);

        var imageUrl = await fileStorage.SaveAsync(file, "menu-items", fileName);
        item.SetImage(imageUrl);

        await menuItemRepository.SaveChangesAsync();

        return item.ToResponse();
    }

    public async Task<IReadOnlyList<MenuItemResponse>> GetByGroupAsync(Guid groupId)
    {
        var items = await menuItemRepository.GetByGroupAsync(groupId);
        var responses = new List<MenuItemResponse>();

        foreach (var item in items)
        {
            var modifierGroups = await modifierGroupRepository.GetByMenuItemAsync(item.Id);
            var modifierGroupResponses = modifierGroups.Select(g => g.ToResponse()).ToList();
            responses.Add(item.ToResponse(modifierGroupResponses));
        }

        return responses;
    }
}
