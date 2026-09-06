namespace Library.Application.DTOs.Users;

public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Points { get; set; }
    public decimal TotalFines { get; set; }
    public DateTime JoinDate { get; set; }
    public int ActiveRentalsCount { get; set; }
    public int TotalBooksRead { get; set; }
    public List<RecentRentalDto> RecentRentals { get; set; } = new();
}

public class RecentRentalDto
{
    public Guid RentalId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Fine { get; set; }
}