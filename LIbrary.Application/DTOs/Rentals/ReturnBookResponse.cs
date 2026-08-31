namespace Library.Application.DTOs.Rentals;

public class ReturnBookResponse
{
    public Guid RentalId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysOverdue { get; set; }
    public decimal FineAmount { get; set; }
    public int PointsEarned { get; set; }
    public int TotalUserPoints { get; set; }
    public decimal TotalUserFines { get; set; }
}