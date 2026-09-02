namespace Library.Application.DTOs.Gamification;

public class LeaderboardEntryDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Points { get; set; }
    public decimal TotalFines { get; set; }
    public int TotalBooksRead { get; set; }
    public int Position { get; set; }
}