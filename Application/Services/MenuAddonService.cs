using Application.DTOs;
using Application.Exceptions;
using Application.Mappers;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;

namespace Application.Services;

public class MenuAddonService(
    IMenuAddonRepository menuAddonRepository,
    IMenuItemRepository menuItemRepository)
{
    public async Task<MenuAddonResponse> CreateAsync(
        CreateMenuAddonRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await menuAddonRepository.NameExistsAsync(
                request.Name,
                cancellationToken: cancellationToken))
        {
            throw new DomainException("A menu add-on with the same name already exists.");
        }

        var addon = MenuAddon.Create(
            request.Name,
            request.Description,
            request.Price,
            request.DisplayOrder);
        await menuAddonRepository.AddAsync(addon, cancellationToken);
        await menuAddonRepository.SaveChangesAsync(cancellationToken);

        return addon.ToResponse([]);
    }

    public async Task<MenuAddonResponse> UpdateAsync(
        Guid id,
        UpdateMenuAddonRequest request,
        CancellationToken cancellationToken = default)
    {
        var addon = await GetAddonAsync(id, cancellationToken);

        if (await menuAddonRepository.NameExistsAsync(
                request.Name,
                id,
                cancellationToken))
        {
            throw new DomainException("A menu add-on with the same name already exists.");
        }

        addon.Update(
            request.Name,
            request.Description,
            request.Price,
            request.DisplayOrder);
        await menuAddonRepository.SaveChangesAsync(cancellationToken);

        return await MapAsync(addon, cancellationToken);
    }

    public async Task SetAvailabilityAsync(
        Guid id,
        bool isAvailable,
        CancellationToken cancellationToken = default)
    {
        var addon = await GetAddonAsync(id, cancellationToken);
        addon.ChangeAvailability(isAvailable);
        await menuAddonRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MenuAddonResponse>> GetAllAsync(
        bool includeUnavailable = false,
        Guid? menuItemId = null,
        CancellationToken cancellationToken = default)
    {
        var addons = await menuAddonRepository.GetAllAsync(
            includeUnavailable,
            menuItemId,
            cancellationToken);
        var responses = new List<MenuAddonResponse>(addons.Count);

        foreach (var addon in addons)
            responses.Add(await MapAsync(addon, cancellationToken));

        return responses;
    }

    public async Task<MenuAddonResponse> ReplaceApplicabilityAsync(
        Guid id,
        ReplaceMenuAddonApplicabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var addon = await GetAddonAsync(id, cancellationToken);
        var menuItemIds = request.MenuItemIds
            .Distinct()
            .ToList();

        foreach (var menuItemId in menuItemIds)
        {
            _ = await menuItemRepository.GetByIdAsync(menuItemId, cancellationToken)
                ?? throw new NotFoundException(
                    $"Menu item with id '{menuItemId}' was not found.");
        }

        var existing = (await menuAddonRepository.GetApplicableMenuItemIdsAsync(
                id,
                cancellationToken))
            .ToHashSet();

        foreach (var menuItemId in existing.Except(menuItemIds))
        {
            var applicability = await menuAddonRepository.GetApplicabilityAsync(
                id,
                menuItemId,
                cancellationToken);
            if (applicability is not null)
                menuAddonRepository.RemoveApplicability(applicability);
        }

        foreach (var menuItemId in menuItemIds.Except(existing))
        {
            await menuAddonRepository.AddApplicabilityAsync(
                MenuAddonMenuItem.Create(id, menuItemId),
                cancellationToken);
        }

        await menuAddonRepository.SaveChangesAsync(cancellationToken);
        return await MapAsync(addon, cancellationToken);
    }

    public async Task<IReadOnlyList<MenuAddon>> GetApplicableAvailableAddonsAsync(
        Guid menuItemId,
        CancellationToken cancellationToken = default) =>
        await menuAddonRepository.GetAllAsync(
            includeUnavailable: false,
            menuItemId,
            cancellationToken);

    private async Task<MenuAddon> GetAddonAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await menuAddonRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Menu add-on with id '{id}' was not found.");

    private async Task<MenuAddonResponse> MapAsync(
        MenuAddon addon,
        CancellationToken cancellationToken)
    {
        var applicableMenuItemIds = await menuAddonRepository.GetApplicableMenuItemIdsAsync(
            addon.Id,
            cancellationToken);
        return addon.ToResponse(applicableMenuItemIds);
    }
}
