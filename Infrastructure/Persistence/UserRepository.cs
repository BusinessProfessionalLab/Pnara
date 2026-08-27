using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default) =>
        await dbContext.Users.FirstOrDefaultAsync(user => user.PhoneNumber == phoneNumber, cancellationToken);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default) =>
        await dbContext.Users.AnyAsync(user => user.PhoneNumber == phoneNumber, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await dbContext.Users.AddAsync(user, cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllAsync(Guid? roleId = null, CancellationToken cancellationToken = default) =>
        await dbContext.Users
            .Where(user => roleId == null || user.RoleId == roleId)
            .OrderBy(user => user.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
