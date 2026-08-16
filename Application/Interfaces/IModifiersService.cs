using Application.DTOs;


namespace Application.Interfaces
{
    public interface IModifiersService
    {
        Task<ModifierDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ModifierDto>> GetByMenuItemIdAsync(Guid menuItemId, CancellationToken cancellationToken = default);
        Task<ModifierDto> CreateAsync(CreateModifierRequest request, CancellationToken cancellationToken = default);
        Task<ModifierDto?> UpdateAsync(Guid id, UpdateModifierRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
