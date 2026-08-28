namespace Library.Application.DTOs.Books;

public class BookDetailsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public string Genre { get; set; } = string.Empty;
    public int AvailableCopies { get; set; }
    public int TotalCopies { get; set; }
    public bool IsAvailable => AvailableCopies > 0;
}