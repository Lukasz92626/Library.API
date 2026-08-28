using FluentValidation;
using Library.Application.DTOs.Books;

namespace Library.Application.Validators;

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required")
            .MaximumLength(100).WithMessage("Author must not exceed 100 characters");

        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required")
            .Length(13).WithMessage("ISBN must be exactly 13 characters")
            .Matches(@"^\d{13}$").WithMessage("ISBN must contain only digits");

        RuleFor(x => x.PublicationYear)
            .InclusiveBetween(1000, DateTime.UtcNow.Year)
            .WithMessage($"Publication year must be between 1000 and {DateTime.UtcNow.Year}");

        RuleFor(x => x.Genre)
            .NotEmpty().WithMessage("Genre is required")
            .MaximumLength(50).WithMessage("Genre must not exceed 50 characters");

        RuleFor(x => x.TotalCopies)
            .GreaterThan(0).WithMessage("Total copies must be greater than 0");
    }
}