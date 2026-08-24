using Library.Domain.Entities;
using Library.Domain.Interfaces;
using Library.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly ApplicationDbContext _context;

    public BookRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Books.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Books.ToListAsync(cancellationToken);

    public Task<IQueryable<Book>> GetQueryable() => Task.FromResult(_context.Books.AsQueryable());

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
        => await _context.Books.AddAsync(book, cancellationToken);

    public Task UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        _context.Books.Update(book);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await GetByIdAsync(id, cancellationToken);
        if (book != null)
        {
            _context.Books.Remove(book);
        }
    }

    public async Task<bool> IsbnExistsAsync(string isbn, CancellationToken cancellationToken = default)
        => await _context.Books.AnyAsync(b => b.ISBN == isbn, cancellationToken);
}