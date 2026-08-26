using MediatR;
using Library.Application.DTOs.Books;

namespace Library.Application.Books.Queries;

public class GetBooksQuery : IRequest<BookListResponse>
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Genre { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}