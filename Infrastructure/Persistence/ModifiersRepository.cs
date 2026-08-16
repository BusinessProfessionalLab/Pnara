using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ModifiersRepository : IModifiersRepository
    {
        private readonly AppDbContext _context;

        public ModifiersRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Modifiers?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Modifiers
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Modifiers>> GetByMenuItemIdAsync(Guid menuItemId, CancellationToken cancellationToken = default)
        {
            return await _context.Modifiers
                .Where(m => m.MenuItemId == menuItemId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Modifiers.AnyAsync(m => m.Id == id, cancellationToken);
        }

        public async Task AddAsync(Modifiers modifier, CancellationToken cancellationToken = default)
        {
            await _context.Modifiers.AddAsync(modifier, cancellationToken);
        }

        public void Update(Modifiers modifier)
        {
            _context.Modifiers.Update(modifier);
        }

        public void Remove(Modifiers modifier)
        {
            _context.Modifiers.Remove(modifier);
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

    }
}
