using Library.Domain.Entities;

namespace Library.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetTopUsersAsync(int count, CancellationToken cancellationToken = default);
    IQueryable<User> GetQueryable();
    IQueryable<User> GetQueryableWithRentals();
}