using Library.Application.DTOs.Gamification;
using Library.Domain.Interfaces;
using Library.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Library.Application.Gamification.Queries;

public class GetLeaderboardQuery : IRequest<LeaderboardResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, LeaderboardResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRentalRepository _rentalRepository;

    public GetLeaderboardQueryHandler(
        IUserRepository userRepository,
        IRentalRepository rentalRepository)
    {
        _userRepository = userRepository;
        _rentalRepository = rentalRepository;
    }

    public async Task<LeaderboardResponse> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var usersQuery = _userRepository.GetQueryable();
        var totalCount = await usersQuery.CountAsync(cancellationToken);
        var users = await usersQuery
            .AsNoTracking()
            .OrderByDescending(u => u.Points)
            .ThenBy(u => u.TotalFines)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        var entries = new List<LeaderboardEntryDto>();
        var position = (request.PageNumber - 1) * request.PageSize + 1;

        foreach (var user in users)
        {
            var rentals = await _rentalRepository.GetRentalsByUserIdAsync(user.Id, cancellationToken);
            var totalBooksRead = rentals.Count(r => r.Status == RentalStatus.Returned);

            entries.Add(new LeaderboardEntryDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Points = user.Points,
                TotalFines = user.TotalFines,
                TotalBooksRead = totalBooksRead,
                Position = position
            });

            position++;
        }

        return new LeaderboardResponse
        {
            Entries = entries,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}