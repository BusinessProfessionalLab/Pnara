using Domain.Entities;

namespace Domain.Repositories;

public interface IUserAddressRepository
{
    Task<UserAddress?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAddress>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserAddress address, CancellationToken cancellationToken = default);
    void Remove(UserAddress address);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
