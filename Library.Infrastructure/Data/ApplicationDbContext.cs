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
    public DbSet<User> Users { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Book configuration
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(200);
            entity.Property(b => b.Author).IsRequired().HasMaxLength(100);
            entity.Property(b => b.ISBN).IsRequired().HasMaxLength(13);
            entity.HasIndex(b => b.ISBN).IsUnique();
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.JoinDate).IsRequired();
        });

        // Rental configuration
        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.RentalDate).IsRequired();
            entity.Property(r => r.DueDate).IsRequired();
            entity.Property(r => r.Status).IsRequired();
            
            entity.HasOne(r => r.User)
                .WithMany(u => u.Rentals)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserActivityLog configuration
        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Action).IsRequired().HasMaxLength(50);
            entity.Property(l => l.Timestamp).IsRequired();
            entity.Property(l => l.Metadata).HasMaxLength(500);
            
            entity.HasOne(l => l.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed data
        SeedData(modelBuilder);
    }
    
    private void SeedData(ModelBuilder modelBuilder)
    {
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