using Library.Domain.Entities;
using Library.Domain.Interfaces;

namespace Library.Infrastructure.Services;

public class FineCalculator : IFineCalculator
{
    private const decimal FinePerDay = 1.0m;
    private const int PointsForOnTimeReturn = 10;
    private const int PointsForLateReturn = 5;

    public decimal CalculateFine(Rental rental)
    {
        if (rental.ReturnDate == null || rental.ReturnDate <= rental.DueDate)
            return 0;

        var daysOverdue = (rental.ReturnDate.Value - rental.DueDate).Days;
        return daysOverdue * FinePerDay;
    }

    public int CalculatePoints(Rental rental)
    {
        if (rental.ReturnDate == null)
            return 0;

        // On time return give 10 points
        if (rental.ReturnDate <= rental.DueDate)
            return PointsForOnTimeReturn;

        // Returning after gives 5 points
        return PointsForLateReturn;
    }
}