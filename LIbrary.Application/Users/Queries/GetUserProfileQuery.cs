using Library.Application.DTOs.Users;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;
using Library.Domain.Entities;
using MediatR;

namespace Library.Application.Users.Queries;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileResponse>;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRentalRepository _rentalRepository;

    public GetUserProfileQueryHandler(
        IUserRepository userRepository,
        IRentalRepository rentalRepository)
    {
        _userRepository = userRepository;
        _rentalRepository = rentalRepository;
    }

    public async Task<UserProfileResponse> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        // Fetches user data
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new UserNotFoundException(request.UserId);
        var rentals = await _rentalRepository.GetRentalsByUserIdAsync(request.UserId, cancellationToken);
        var rentalsList = rentals.ToList();
        
        var activeRentals = rentalsList.Where(r => r.Status == RentalStatus.Active).ToList();
        var returnedRentals = rentalsList.Where(r => r.Status == RentalStatus.Returned).ToList();
        
        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Points = user.Points,
            TotalFines = user.TotalFines,
            JoinDate = user.JoinDate,
            ActiveRentalsCount = activeRentals.Count,
            TotalBooksRead = returnedRentals.Count,
            RecentRentals = rentalsList
                .OrderByDescending(r => r.RentalDate)
                .Take(5)
                .Select(r => new RecentRentalDto
                {
                    RentalId = r.Id,
                    BookTitle = r.Book?.Title ?? "Unknown",
                    RentalDate = r.RentalDate,
                    ReturnDate = r.ReturnDate,
                    Status = r.Status.ToString(),
                    Fine = r.ReturnDate > r.DueDate 
                        ? (r.ReturnDate.Value - r.DueDate).Days * 1.0m 
                        : null
                })
                .ToList()
        };
    }
}