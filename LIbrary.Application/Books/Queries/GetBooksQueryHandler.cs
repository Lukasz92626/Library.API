using AutoMapper;
using Library.Application.Books;
using Library.Domain.Interfaces;
using MediatR;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Library.Application.DTOs.Books;

namespace Library.Application.Books.Queries;

public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, BookListResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public GetBooksQueryHandler(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<BookListResponse> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        var query = _bookRepository.GetQueryable();

        // Filtration
        if (!string.IsNullOrWhiteSpace(request.Title))
            query = query.Where(b => b.Title.Contains(request.Title));
        if (!string.IsNullOrWhiteSpace(request.Author))
            query = query.Where(b => b.Author.Contains(request.Author));
        if (!string.IsNullOrWhiteSpace(request.Genre))
            query = query.Where(b => b.Genre.Contains(request.Genre));
        
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new BookListResponse
        {
            Items = _mapper.Map<List<BookDto>>(items),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}