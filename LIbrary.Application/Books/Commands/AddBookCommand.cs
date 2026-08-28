using MediatR;
using Library.Application.DTOs.Books;

namespace Library.Application.Books.Commands;

public record AddBookCommand(
    string Title,
    string Author,
    string ISBN,
    int PublicationYear,
    string Genre,
    int TotalCopies) : IRequest<BookDetailsDto>;