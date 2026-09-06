using AutoMapper;
using Library.Application.DTOs.Books;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;
using MediatR;

namespace Library.Application.Books.Queries;

public record GetBookByIdQuery(Guid Id) : IRequest<BookDetailsDto>;

public class GetBookByIdQueryHandler
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public GetBookByIdQueryHandler(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<BookDetailsDto> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.Id, cancellationToken);
        if (book == null)
            throw new BookNotFoundException(request.Id);

        return _mapper.Map<BookDetailsDto>(book);
    }
}