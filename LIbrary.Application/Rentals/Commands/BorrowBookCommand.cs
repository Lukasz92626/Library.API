using MediatR;
using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;

namespace Library.Application.Rentals.Commands;

public record BorrowBookCommand(Guid UserId, Guid BookId) : IRequest<Guid>;

public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, Guid>
{
    private readonly IBookRepository _bookRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public BorrowBookCommandHandler(
        IBookRepository bookRepository,
        IRentalRepository rentalRepository,
        IUnitOfWork unitOfWork)
    {
        _bookRepository = bookRepository;
        _rentalRepository = rentalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        // Checks the availability of the book
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book == null) throw new BookNotFoundException(request.BookId);
        if (book.AvailableCopies <= 0) throw new BookUnavailableException(request.BookId);

        // Checks the loan limit
        var userRentals = await _rentalRepository.GetActiveRentalsByUserIdAsync(request.UserId, cancellationToken);
        if (userRentals.Count() >= 5) throw new RentalLimitExceededException(5);

        // Creates a new rental
        var rental = new Rental
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            BookId = request.BookId,
            RentalDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = RentalStatus.Active
        };
        
        book.AvailableCopies--;

        // Saves changes to the transaction
        await _rentalRepository.AddAsync(rental, cancellationToken);
        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return rental.Id;
    }
}