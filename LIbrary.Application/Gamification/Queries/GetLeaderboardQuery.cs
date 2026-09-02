using MediatR;
using Library.Application.DTOs.Gamification;

namespace Library.Application.Gamification.Queries;

public class GetLeaderboardQuery : IRequest<LeaderboardResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}