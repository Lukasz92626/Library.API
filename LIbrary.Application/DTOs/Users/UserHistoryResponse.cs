namespace Library.Application.DTOs.Users;

public class UserHistoryResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int TotalRentals { get; set; }
    public int TotalBooksRead { get; set; }
    public int ActiveRentals { get; set; }
    public decimal TotalFines { get; set; }
    public int Points { get; set; }
    public List<UserHistoryRentalDto> Rentals { get; set; } = new();
}

public class UserHistoryRentalDto
{
    public Guid RentalId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public DateTime RentalDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Fine { get; set; }
    public bool IsOverdue { get; set; }
}