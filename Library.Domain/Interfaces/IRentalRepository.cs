using Library.Domain.Entities;

namespace Library.Domain.Interfaces;

public interface IRentalRepository
{
    Task<Rental?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Rental>> GetActiveRentalsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Rental>> GetRentalsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Rental>> GetOverdueRentalsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Rental rental, CancellationToken cancellationToken = default);
    Task UpdateAsync(Rental rental, CancellationToken cancellationToken = default);
}