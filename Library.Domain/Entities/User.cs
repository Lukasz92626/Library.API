namespace Library.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Points { get; set; }
    public decimal TotalFines { get; set; }
    public DateTime JoinDate { get; set; }
    public List<Rental> Rentals { get; set; } = new();
    public List<UserActivityLog> ActivityLogs { get; set; } = new();
    public string Role { get; set; } = "User";
}