namespace Library.Domain.Exceptions;

public class BookNotFoundException : DomainException
{
    public BookNotFoundException() : base("Book not found.") { }
    public BookNotFoundException(Guid bookId) : base($"Book with ID '{bookId}' was not found.") { }
    public BookNotFoundException(string isbn) : base($"Book with ISBN '{isbn}' was not found.") { }
}