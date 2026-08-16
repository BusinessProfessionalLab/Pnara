using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services
{
    public class ModifiersService : IModifiersService
    {
        private readonly IModifiersRepository _repository;

        public ModifiersService(IModifiersRepository repository)
        {
            _repository = repository;
        }

        public async Task<ModifierDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var modifier = await _repository.GetByIdAsync(id, cancellationToken);
            return modifier is null ? null : MapToDto(modifier);
        }

        public async Task<IReadOnlyList<ModifierDto>> GetByMenuItemIdAsync(Guid menuItemId, CancellationToken cancellationToken = default)
        {
            var modifiers = await _repository.GetByMenuItemIdAsync(menuItemId, cancellationToken);
            return modifiers.Select(MapToDto).ToList();
        }

        public async Task<ModifierDto> CreateAsync(CreateModifierRequest request, CancellationToken cancellationToken = default)
        {
            // توجه: ترتیب آرگومان‌ها مطابق امضای فعلی Create است (description قبل از title)
            var modifier = Modifiers.Create(request.MenuItemId, request.Description ?? string.Empty, request.Title);

            await _repository.AddAsync(modifier, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(modifier);
        }

        public async Task<ModifierDto?> UpdateAsync(Guid id, UpdateModifierRequest request, CancellationToken cancellationToken = default)
        {
            var modifier = await _repository.GetByIdAsync(id, cancellationToken);
            if (modifier is null)
                return null;

            modifier.Update(request.Title, request.Description ?? string.Empty, request.IsAvailable);

            _repository.Update(modifier);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(modifier);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var modifier = await _repository.GetByIdAsync(id, cancellationToken);
            if (modifier is null)
                return false;

            _repository.Remove(modifier);
            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }

        private static ModifierDto MapToDto(Modifiers modifier)
        {
            return new ModifierDto(
                modifier.Id,
                modifier.MenuItemId,
                modifier.Title,
                modifier.Description,
                modifier.IsAvailable);
        }
    }
}
