using Library.Domain.Entities;

namespace Library.Domain.Interfaces;

public interface IFineCalculator
{
    decimal CalculateFine(Rental rental);
    int CalculatePoints(Rental rental);
}