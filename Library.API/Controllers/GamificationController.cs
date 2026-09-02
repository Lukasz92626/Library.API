using Library.Application.DTOs.Gamification;
using Library.Application.Gamification.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}