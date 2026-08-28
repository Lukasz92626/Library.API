using MediatR;
using Library.Application.DTOs.Books;

namespace Library.Application.Books.Queries;

public record GetBookByIdQuery(Guid Id) : IRequest<BookDetailsDto>;