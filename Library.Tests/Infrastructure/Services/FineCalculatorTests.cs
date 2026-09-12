using FluentAssertions;
using Library.Domain.Entities;
using Library.Infrastructure.Services;

namespace Library.Tests.Infrastructure.Services;

public class FineCalculatorTests
{
    private readonly FineCalculator _calculator;

    public FineCalculatorTests()
    {
        _calculator = new FineCalculator();
    }

    [Fact]
    public void CalculateFine_WhenReturnedOnTime_ShouldReturnZero()
    {
        // Arrange
        var rental = new Rental
        {
            DueDate = DateTime.UtcNow.AddDays(5),
            ReturnDate = DateTime.UtcNow
        };

        // Act
        var fine = _calculator.CalculateFine(rental);

        // Assert
        fine.Should().Be(0);
    }

    [Fact]
    public void CalculateFine_WhenReturnedLate_ShouldReturnFinePerDay()
    {
        // Arrange
        var rental = new Rental
        {
            DueDate = DateTime.UtcNow.AddDays(-3),
            ReturnDate = DateTime.UtcNow
        };

        // Act
        var fine = _calculator.CalculateFine(rental);

        // Assert
        fine.Should().Be(3.0m);
    }

    [Fact]
    public void CalculatePoints_WhenReturnedOnTime_ShouldReturn10Points()
    {
        // Arrange
        var rental = new Rental
        {
            DueDate = DateTime.UtcNow.AddDays(5),
            ReturnDate = DateTime.UtcNow
        };

        // Act
        var points = _calculator.CalculatePoints(rental);

        // Assert
        points.Should().Be(10);
    }

    [Fact]
    public void CalculatePoints_WhenReturnedLate_ShouldReturn5Points()
    {
        // Arrange
        var rental = new Rental
        {
            DueDate = DateTime.UtcNow.AddDays(-3),
            ReturnDate = DateTime.UtcNow
        };

        // Act
        var points = _calculator.CalculatePoints(rental);

        // Assert
        points.Should().Be(5);
    }
}