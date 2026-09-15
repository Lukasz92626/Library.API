using Library.Domain.Entities;

namespace Library.Domain.Interfaces;

public interface IActivityLogRepository
{
    Task AddAsync(UserActivityLog log, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserActivityLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}