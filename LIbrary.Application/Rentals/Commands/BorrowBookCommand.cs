using MediatR;
using Library.Application.DTOs.Rentals;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;

using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Library.Application.Rentals.Commands;

public record BorrowBookCommand(Guid UserId, Guid BookId) : IRequest<RentalResponse>;

public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, RentalResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActivityLogger _activityLogger;
    private readonly IMapper _mapper;
    private readonly ILogger<BorrowBookCommandHandler> _logger;
    
    private const int MaxRentals = 5;
    private const int RentalDays = 30;
    
    public BorrowBookCommandHandler(
        IBookRepository bookRepository,
        IRentalRepository rentalRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IActivityLogger activityLogger,
        IMapper mapper,
        ILogger<BorrowBookCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _rentalRepository = rentalRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<RentalResponse> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
                throw new UserNotFoundException(request.UserId);
            
            var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
            if (book == null)
                throw new BookNotFoundException(request.BookId);

            if (book.AvailableCopies <= 0)
                throw new BookUnavailableException(request.BookId);
            
            var activeRentals = await _rentalRepository.GetActiveRentalsByUserIdAsync(request.UserId, cancellationToken);
            var activeRentalsList = activeRentals.ToList();
            
            if (activeRentalsList.Count >= MaxRentals)
                throw new RentalLimitExceededException(MaxRentals);
            
            var rental = new Rental
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                BookId = request.BookId,
                RentalDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(RentalDays),
                Status = RentalStatus.Active
            };
            
            book.AvailableCopies--;
            
            await _rentalRepository.AddAsync(rental, cancellationToken);
            await _bookRepository.UpdateAsync(book, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            await _activityLogger.LogAsync(request.UserId, "Borrowed", new
            {
                RentalId = rental.Id,
                BookId = book.Id,
                BookTitle = book.Title,
                DueDate = rental.DueDate
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} borrowed book {BookId} (Rental id: {RentalId})", 
                request.UserId, request.BookId, rental.Id);
            
            var response = _mapper.Map<RentalResponse>(rental);
            response.BookTitle = book.Title;

            return response;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}