namespace Library.Domain.Exceptions;

public class RentalLimitExceededException : DomainException
{
    public RentalLimitExceededException() : base("User has reached the maximum number of rentals.") { }
    public RentalLimitExceededException(int maxRentals) : base($"User cannot rent more than {maxRentals} books at a time.") { }
}