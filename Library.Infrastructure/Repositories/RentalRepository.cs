using Library.Domain.Entities;
using Library.Domain.Interfaces;
using Library.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly ApplicationDbContext _context;

    public RentalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Rental?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Rentals
            .Include(r => r.Book)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IEnumerable<Rental>> GetActiveRentalsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Rentals
            .Where(r => r.UserId == userId && r.Status == RentalStatus.Active)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Rental>> GetRentalsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Rentals
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RentalDate)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Rental>> GetOverdueRentalsAsync(CancellationToken cancellationToken = default)
        => await _context.Rentals
            .Where(r => r.Status == RentalStatus.Active && r.DueDate < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Rental rental, CancellationToken cancellationToken = default)
        => await _context.Rentals.AddAsync(rental, cancellationToken);

    public Task UpdateAsync(Rental rental, CancellationToken cancellationToken = default)
    {
        _context.Rentals.Update(rental);
        return Task.CompletedTask;
    }
}