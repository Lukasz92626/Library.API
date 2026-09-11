using Library.Application.DTOs.Rentals;
using Library.Application.Rentals.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Library.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RentalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RentalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("borrow")]
    public async Task<ActionResult<RentalResponse>> BorrowBook(BorrowBookRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        
        var command = new BorrowBookCommand(userId, request.BookId);
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }
    
    [HttpPost("return/{rentalId}")]
    public async Task<ActionResult<ReturnBookResponse>> ReturnBook(Guid rentalId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        
        var command = new ReturnBookCommand(userId, rentalId);
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }
    
    [HttpPut("renew/{rentalId}")]
    public async Task<ActionResult<RenewRentalResponse>> RenewRental(Guid rentalId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        var command = new RenewRentalCommand(userId, rentalId);
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }
}