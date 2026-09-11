using Library.Application.DTOs.Users;
using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;
using MediatR;

namespace Library.Application.Users.Queries;

public record GetUserHistoryQuery(Guid UserId) : IRequest<UserHistoryResponse>;

public class GetUserHistoryQueryHandler : IRequestHandler<GetUserHistoryQuery, UserHistoryResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRentalRepository _rentalRepository;

    public GetUserHistoryQueryHandler(
        IUserRepository userRepository,
        IRentalRepository rentalRepository)
    {
        _userRepository = userRepository;
        _rentalRepository = rentalRepository;
    }

    public async Task<UserHistoryResponse> Handle(GetUserHistoryQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new UserNotFoundException(request.UserId);
        
        var rentals = await _rentalRepository.GetRentalsByUserIdAsync(request.UserId, cancellationToken);
        var rentalsList = rentals.ToList();
        
        var rentalDtos = rentalsList
            .OrderByDescending(r => r.RentalDate)
            .Select(r => new UserHistoryRentalDto
            {
                RentalId = r.Id,
                BookTitle = r.Book?.Title ?? "Unknown",
                BookAuthor = r.Book?.Author ?? "Unknown",
                RentalDate = r.RentalDate,
                DueDate = r.DueDate,
                ReturnDate = r.ReturnDate,
                Status = r.Status.ToString(),
                IsOverdue = r.Status == RentalStatus.Active && r.DueDate < DateTime.UtcNow,
                Fine = r.ReturnDate.HasValue && r.ReturnDate > r.DueDate
                    ? (r.ReturnDate.Value - r.DueDate).Days * 1.0m
                    : 0
            })
            .ToList();

        return new UserHistoryResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Points = user.Points,
            TotalFines = user.TotalFines,
            TotalRentals = rentalsList.Count,
            TotalBooksRead = rentalsList.Count(r => r.Status == RentalStatus.Returned),
            ActiveRentals = rentalsList.Count(r => r.Status == RentalStatus.Active),
            Rentals = rentalDtos
        };
    }
}