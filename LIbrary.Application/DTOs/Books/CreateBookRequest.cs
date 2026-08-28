namespace Library.Application.DTOs.Books;

public class CreateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public string Genre { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
}