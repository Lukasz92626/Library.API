namespace Library.Domain.Entities;

public class Rental
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }
    public DateTime RentalDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public RentalStatus Status { get; set; }
    public int RenewalCount { get; set; } = 0;
    public User User { get; set; } = null!;
    public Book Book { get; set; } = null!;
}

public enum RentalStatus
{
    Active = 0,
    Returned = 1,
    Overdue = 2
}