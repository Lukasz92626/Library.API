using Library.Application.Books.Commands;
using Library.Application.Books.Queries;
using Library.Application.DTOs.Books;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public BooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<BookListResponse>> GetBooks(
        [FromQuery] string? title,
        [FromQuery] string? author,
        [FromQuery] string? genre,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetBooksQuery
        {
            Title = title,
            Author = author,
            Genre = genre,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<BookDetailsDto>> GetBookById(Guid id)
    {
        var query = new GetBookByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookDetailsDto>> AddBook(CreateBookRequest request)
    {
        var command = new AddBookCommand(
            request.Title,
            request.Author,
            request.ISBN,
            request.PublicationYear,
            request.Genre,
            request.TotalCopies
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetBookById), new { id = result.Id }, result);
    }
}