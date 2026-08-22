using Application.DTOs;
using Application.Exceptions;
using Application.Mappers;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;

namespace Application.Services;

public class ModifierGroupService(IModifierGroupRepository modifierGroupRepository, IMenuItemRepository menuItemRepository)
{
    public async Task<ModifierGroupResponse> CreateAsync(CreateModifierGroupRequest request)
    {
        var selectionType = ParseSelectionType(request.SelectionType);

        var group = ModifierGroup.Create(request.Name, selectionType, request.MinSelection, request.MaxSelection, request.IsRequired);

        await modifierGroupRepository.AddAsync(group);
        await modifierGroupRepository.SaveChangesAsync();

        return group.ToResponse();
    }

    public async Task<ModifierGroupResponse> UpdateAsync(Guid id, UpdateModifierGroupRequest request)
    {
        var group = await modifierGroupRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Modifier group with id '{id}' was not found.");

        var selectionType = ParseSelectionType(request.SelectionType);

        group.Update(request.Name, selectionType, request.MinSelection, request.MaxSelection, request.IsRequired);

        await modifierGroupRepository.SaveChangesAsync();

        return group.ToResponse();
    }

    public async Task<ModifierGroupResponse> GetByIdAsync(Guid id)
    {
        var group = await modifierGroupRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Modifier group with id '{id}' was not found.");

        return group.ToResponse();
    }

    public async Task<IReadOnlyList<ModifierGroupResponse>> GetAllAsync()
    {
        var groups = await modifierGroupRepository.GetAllAsync();
        return groups.Select(g => g.ToResponse()).ToList();
    }

    public async Task<ModifierResponse> AddModifierAsync(Guid groupId, CreateModifierRequest request)
    {
        var group = await modifierGroupRepository.GetByIdAsync(groupId)
            ?? throw new NotFoundException($"Modifier group with id '{groupId}' was not found.");

        var modifier = group.AddModifier(request.Name, request.Price, request.DisplayOrder);
        await modifierGroupRepository.AddModifierAsync(modifier);
        await modifierGroupRepository.SaveChangesAsync();

        return modifier.ToResponse();
    }

    public async Task<ModifierResponse> UpdateModifierAsync(Guid groupId, Guid modifierId, UpdateModifierRequest request)
    {
        var group = await modifierGroupRepository.GetByIdAsync(groupId)
            ?? throw new NotFoundException($"Modifier group with id '{groupId}' was not found.");

        var modifier = group.GetModifierById(modifierId)
            ?? throw new NotFoundException($"Modifier with id '{modifierId}' was not found in group '{groupId}'.");

        modifier.Update(request.Name, request.Price, request.DisplayOrder);

        await modifierGroupRepository.SaveChangesAsync();

        return modifier.ToResponse();
    }

    public async Task RemoveModifierAsync(Guid groupId, Guid modifierId)
    {
        var group = await modifierGroupRepository.GetByIdAsync(groupId)
            ?? throw new NotFoundException($"Modifier group with id '{groupId}' was not found.");

        var modifier = group.GetModifierById(modifierId)
            ?? throw new NotFoundException($"Modifier with id '{modifierId}' was not found in group '{groupId}'.");

        group.RemoveModifier(modifier);

        await modifierGroupRepository.SaveChangesAsync();
    }

    public async Task ChangeModifierAvailabilityAsync(Guid groupId, Guid modifierId, bool isAvailable)
    {
        var group = await modifierGroupRepository.GetByIdAsync(groupId)
            ?? throw new NotFoundException($"Modifier group with id '{groupId}' was not found.");

        var modifier = group.GetModifierById(modifierId)
            ?? throw new NotFoundException($"Modifier with id '{modifierId}' was not found in group '{groupId}'.");

        modifier.ChangeAvailability(isAvailable);

        await modifierGroupRepository.SaveChangesAsync();
    }

    public async Task AttachToMenuItemAsync(Guid groupId, Guid menuItemId)
    {
        var group = await modifierGroupRepository.GetByIdAsync(groupId)
            ?? throw new NotFoundException($"Modifier group with id '{groupId}' was not found.");

        var menuItem = await menuItemRepository.GetByIdAsync(menuItemId)
            ?? throw new NotFoundException($"Menu item with id '{menuItemId}' was not found.");

        group.AttachMenuItem(menuItemId);

        await modifierGroupRepository.SaveChangesAsync();
    }

    public async Task DetachFromMenuItemAsync(Guid groupId, Guid menuItemId)
    {
        var group = await modifierGroupRepository.GetByIdAsync(groupId)
            ?? throw new NotFoundException($"Modifier group with id '{groupId}' was not found.");

        group.DetachMenuItem(menuItemId);

        await modifierGroupRepository.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ModifierGroupMenuItemResponse>> GetByMenuItemAsync(Guid menuItemId)
    {
        var groups = await modifierGroupRepository.GetByMenuItemAsync(menuItemId);
        return groups.Select(g => new ModifierGroupMenuItemResponse(g.Id, g.Name)).ToList();
    }

    private static SelectionType ParseSelectionType(string selectionType)
    {
        return selectionType?.ToLowerInvariant() switch
        {
            "single" => Domain.Enums.SelectionType.Single,
            "multiple" => Domain.Enums.SelectionType.Multiple,
            _ => throw new DomainException($"Invalid selection type '{selectionType}'. Valid values are: Single, Multiple.")
        };
    }
}
