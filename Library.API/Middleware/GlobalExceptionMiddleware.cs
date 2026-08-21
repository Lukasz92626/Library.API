using System.Text.Json;
using Library.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Library.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            // Domain exceptions - 400 series
            BookNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            UserNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            RentalNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            
            BookUnavailableException => (StatusCodes.Status409Conflict, exception.Message),
            RentalLimitExceededException => (StatusCodes.Status400BadRequest, exception.Message),
            InvalidOperationDomainException => (StatusCodes.Status400BadRequest, exception.Message),
            
            // Security exceptions
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "You are not authorized."),
            
            // Validation exceptions (if you use FluentValidation)
            FluentValidation.ValidationException => (StatusCodes.Status400BadRequest, exception.Message),
            
            // Default - 500
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")
        };

        var response = new
        {
            error = message,
            statusCode = statusCode,
            timestamp = DateTime.UtcNow
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}