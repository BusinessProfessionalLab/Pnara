using Application.DTOs;
using Application.Exceptions;
using Application.Mappers;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

public class MenuGroupService(IMenuGroupRepository menuGroupRepository, IMenuItemRepository menuItemRepository)
{
    public async Task<MenuGroupResponse> CreateAsync(CreateMenuGroupRequest request)
    {
        var group = MenuGroup.Create(request.Name, request.DisplayOrder);

        await menuGroupRepository.AddAsync(group);
        await menuGroupRepository.SaveChangesAsync();

        return group.ToResponse();
    }

    public async Task<MenuGroupResponse> UpdateAsync(Guid id, UpdateMenuGroupRequest request)
    {
        var group = await menuGroupRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Menu group with id '{id}' was not found.");

        group.Update(request.Name, request.DisplayOrder);

        await menuGroupRepository.SaveChangesAsync();

        return group.ToResponse();
    }

    public async Task ActivateAsync(Guid id)
    {
        var group = await menuGroupRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Menu group with id '{id}' was not found.");

        group.Activate();

        await menuGroupRepository.SaveChangesAsync();
    }

    public async Task DeactivateAsync(Guid id)
    {
        var group = await menuGroupRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Menu group with id '{id}' was not found.");

        group.Deactivate();

        await menuGroupRepository.SaveChangesAsync();
    }

    public async Task SetStatusAsync(Guid id, bool isActive)
    {
        var group = await menuGroupRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Menu group with id '{id}' was not found.");

        if (isActive)
            group.Activate();
        else
            group.Deactivate();

        await menuGroupRepository.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<MenuGroupResponse>> GetAllAsync(bool includeInactive = false)
    {
        var groups = await menuGroupRepository.GetAllAsync();

        if (!includeInactive)
            groups = groups.Where(group => group.IsActive).ToList();

        return groups.Select(group => group.ToResponse()).ToList();
    }

    public async Task<PublicMenuResponse> GetPublicMenuAsync()
    {
        var groups = await menuGroupRepository.GetAllAsync();
        var activeGroups = groups.Where(group => group.IsActive).ToList();

        var publicGroups = new List<PublicMenuGroupDto>();

        foreach (var group in activeGroups)
        {
            var items = await menuItemRepository.GetByGroupAsync(group.Id);
            var availableItems = items
                .Where(item => item.IsAvailable)
                .Select(item => new PublicMenuItemDto(
                    item.Id, item.Name, item.Description, item.Price, item.ImageUrl, item.DisplayOrder))
                .ToList();

            publicGroups.Add(new PublicMenuGroupDto(
                group.Id, group.Name, group.DisplayOrder, availableItems));
        }

        return new PublicMenuResponse(publicGroups);
    }
}
