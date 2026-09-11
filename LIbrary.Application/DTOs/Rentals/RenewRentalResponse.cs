namespace Library.Application.DTOs.Rentals;

public class RenewRentalResponse
{
    public Guid RentalId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public DateTime NewDueDate { get; set; }
    public int RenewalCount { get; set; }
    public int MaxRenewals { get; set; }
    public int DaysExtended { get; set; }
}