using Library.Application.DTOs.Rentals;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;

using MediatR;
using Microsoft.Extensions.Logging;

namespace Library.Application.Rentals.Commands;

public record ReturnBookCommand(Guid UserId, Guid RentalId) : IRequest<ReturnBookResponse>;

public class ReturnBookCommandHandler : IRequestHandler<ReturnBookCommand, ReturnBookResponse>
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFineCalculator _fineCalculator;
    private readonly IActivityLogger _activityLogger;
    private readonly ILogger<ReturnBookCommandHandler> _logger;

    public ReturnBookCommandHandler(
        IRentalRepository rentalRepository,
        IBookRepository bookRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IFineCalculator fineCalculator,
        IActivityLogger activityLogger,
        ILogger<ReturnBookCommandHandler> logger)
    {
        _rentalRepository = rentalRepository;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _fineCalculator = fineCalculator;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<ReturnBookResponse> Handle(ReturnBookCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Find a rental
            var rental = await _rentalRepository.GetByIdAsync(request.RentalId, cancellationToken);
            if (rental == null)
            {
                throw new RentalNotFoundException(request.RentalId);
            }

            // Check borrow
            if (rental.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You can only return your own rentals.");
            }
            if (rental.Status == RentalStatus.Returned)
            {
                throw new InvalidOperationDomainException("This book has already been returned.");
            }

            // Find user
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                throw new UserNotFoundException(request.UserId);
            }
            
            // Find book
            var book = await _bookRepository.GetByIdAsync(rental.BookId, cancellationToken);
            if (book == null)
            {
                throw new BookNotFoundException(rental.BookId);
            }

            // Set return date
            rental.ReturnDate = DateTime.UtcNow;
            rental.Status = RentalStatus.Returned;

            // Sets points and fine
            var fine = _fineCalculator.CalculateFine(rental);
            var points = _fineCalculator.CalculatePoints(rental);
            user.TotalFines += fine;
            user.Points += points;

            // Save changes
            book.AvailableCopies++;
            await _rentalRepository.UpdateAsync(rental, cancellationToken);
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _bookRepository.UpdateAsync(book, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Confirm transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            await _activityLogger.LogAsync(request.UserId, "Returned", new
            {
                RentalId = rental.Id,
                BookId = book.Id,
                BookTitle = book.Title,
                Fine = fine,
                PointsEarned = points
            }, cancellationToken);

            if (points > 0)
            {
                await _activityLogger.LogAsync(request.UserId, "EarnedPoints", new
                {
                    Points = points,
                    Reason = "Book returned"
                }, cancellationToken);
            }
            
            _logger.LogInformation(
                "User {UserId} returned book {BookId} (Rental: {RentalId}). Fine: {Fine}, Points: {Points}",
                request.UserId, rental.BookId, rental.Id, fine, points);
            
            var daysOverdue = rental.ReturnDate > rental.DueDate 
                ? (rental.ReturnDate.Value - rental.DueDate).Days 
                : 0;

            return new ReturnBookResponse
            {
                RentalId = rental.Id,
                BookTitle = book.Title,
                ReturnDate = rental.ReturnDate.Value,
                IsOverdue = daysOverdue > 0,
                DaysOverdue = daysOverdue,
                FineAmount = fine,
                PointsEarned = points,
                TotalUserPoints = user.Points,
                TotalUserFines = user.TotalFines
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}