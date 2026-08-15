using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Book entity configuration
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(200);
            entity.Property(b => b.ISBN).IsRequired().HasMaxLength(13);
            entity.HasIndex(b => b.ISBN).IsUnique();
        });

        // Seed of data
        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "First title",
                Author = "First author",
                ISBN = "1111111111111",
                PublicationYear = 2008,
                Genre = "Programming",
                TotalCopies = 5,
                AvailableCopies = 5
            },
            new Book
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Title = "Second title",
                Author = "Second author",
                ISBN = "2222222222222",
                PublicationYear = 2000,
                Genre = "Fantasy",
                TotalCopies = 10,
                AvailableCopies = 10
            }
        );
    }
}