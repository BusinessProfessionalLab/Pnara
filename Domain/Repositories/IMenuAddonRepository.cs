using Domain.Entities;

namespace Domain.Repositories;

public interface IMenuAddonRepository
{
    Task<MenuAddon?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MenuAddon>> GetAllAsync(
        bool includeUnavailable = false,
        Guid? menuItemId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MenuAddon>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        MenuAddon addon,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetApplicableMenuItemIdsAsync(
        Guid addonId,
        CancellationToken cancellationToken = default);

    Task<bool> IsApplicableToMenuItemAsync(
        Guid addonId,
        Guid menuItemId,
        CancellationToken cancellationToken = default);

    Task AddApplicabilityAsync(
        MenuAddonMenuItem applicability,
        CancellationToken cancellationToken = default);

    Task<MenuAddonMenuItem?> GetApplicabilityAsync(
        Guid addonId,
        Guid menuItemId,
        CancellationToken cancellationToken = default);

    void RemoveApplicability(MenuAddonMenuItem applicability);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
