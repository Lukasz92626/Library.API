namespace Library.Domain.Exceptions;

public class RentalNotFoundException : DomainException
{
    public RentalNotFoundException() : base("Rental not found.") { }
    public RentalNotFoundException(Guid rentalId) : base($"Rental with ID '{rentalId}' was not found.") { }
}