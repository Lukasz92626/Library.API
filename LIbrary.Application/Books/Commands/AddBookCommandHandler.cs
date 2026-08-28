using AutoMapper;
using Library.Application.DTOs.Books;
using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;
using MediatR;

namespace Library.Application.Books.Commands;

public class AddBookCommandHandler : IRequestHandler<AddBookCommand, BookDetailsDto>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddBookCommandHandler(IBookRepository bookRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BookDetailsDto> Handle(AddBookCommand request, CancellationToken cancellationToken)
    {
        // Sprawdź czy ISBN już istnieje
        if (await _bookRepository.IsbnExistsAsync(request.ISBN, cancellationToken))
            throw new InvalidOperationDomainException($"Book with ISBN '{request.ISBN}' already exists.");

        var book = _mapper.Map<Book>(request);
        book.Id = Guid.NewGuid();
        book.AvailableCopies = request.TotalCopies;

        await _bookRepository.AddAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BookDetailsDto>(book);
    }
}