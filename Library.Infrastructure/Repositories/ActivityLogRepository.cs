using Library.Domain.Entities;
using Library.Domain.Interfaces;
using Library.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly ApplicationDbContext _context;

    public ActivityLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserActivityLog log, CancellationToken cancellationToken = default) => await _context.UserActivityLogs.AddAsync(log, cancellationToken);

    public async Task<IEnumerable<UserActivityLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.UserActivityLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync(cancellationToken);
}