using Library.Application.DTOs.Gamification;
using Library.Application.Gamification.Queries;
using Library.Application.Gamification.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Library.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GamificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public GamificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("leaderboard")]
    public async Task<ActionResult<LeaderboardResponse>> GetLeaderboard(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetLeaderboardQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize > 50 ? 50 : pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpPost("claim-bonus")]
    public async Task<ActionResult<DailyBonusResponse>> ClaimDailyBonus()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        
        var command = new ClaimDailyBonusCommand(userId);
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }
}