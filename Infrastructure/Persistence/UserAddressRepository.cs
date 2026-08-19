using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class UserAddressRepository(AppDbContext dbContext) : IUserAddressRepository
{
    public async Task<UserAddress?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.UserAddresses.FirstOrDefaultAsync(address => address.Id == id, cancellationToken);

    public async Task<IReadOnlyList<UserAddress>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await dbContext.UserAddresses
            .Where(address => address.UserId == userId)
            .OrderByDescending(address => address.IsDefault)
            .ThenBy(address => address.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(UserAddress address, CancellationToken cancellationToken = default) =>
        await dbContext.UserAddresses.AddAsync(address, cancellationToken);

    public void Remove(UserAddress address) =>
        dbContext.UserAddresses.Remove(address);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
