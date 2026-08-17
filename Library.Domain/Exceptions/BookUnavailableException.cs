namespace Library.Domain.Exceptions;

public class BookUnavailableException : DomainException
{
    public BookUnavailableException() : base("This book is currently unavailable.") { }
    public BookUnavailableException(Guid bookId) : base($"Book with ID '{bookId}' has no available copies.") { }
}