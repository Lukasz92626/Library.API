using Library.Application.DTOs.Users;
using Library.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Library.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetMyProfile()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        var query = new GetUserProfileQuery(userId);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
    
    [HttpGet("{id}/history")]
    public async Task<ActionResult<UserHistoryResponse>> GetUserHistory(Guid id)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        var isAdmin = User.IsInRole("Admin");

        if (id != currentUserId && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to view user's history.");

        var query = new GetUserHistoryQuery(id);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
}