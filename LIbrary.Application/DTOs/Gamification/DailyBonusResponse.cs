namespace Library.Application.DTOs.Gamification;

public class DailyBonusResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int BonusPoints { get; set; }
    public int TotalPoints { get; set; }
    public DateTime NextBonusAvailable { get; set; }
}