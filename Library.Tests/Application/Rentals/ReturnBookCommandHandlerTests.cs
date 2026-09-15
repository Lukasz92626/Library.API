using FluentAssertions;

using Library.Application.Interfaces;
using Library.Application.Rentals.Commands;

using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;

using Microsoft.Extensions.Logging;
using Moq;

namespace Library.Tests.Application.Rentals;

public class ReturnBookCommandHandlerTests
{
    private readonly Mock<IRentalRepository> _rentalRepositoryMock;
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFineCalculator> _fineCalculatorMock;
    private readonly Mock<IActivityLogger> _activityLoggerMock;
    private readonly Mock<ILogger<ReturnBookCommandHandler>> _loggerMock;
    private readonly ReturnBookCommandHandler _handler;

    public ReturnBookCommandHandlerTests()
    {
        _rentalRepositoryMock = new Mock<IRentalRepository>();
        _bookRepositoryMock = new Mock<IBookRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fineCalculatorMock = new Mock<IFineCalculator>();
        _activityLoggerMock = new Mock<IActivityLogger>();
        _loggerMock = new Mock<ILogger<ReturnBookCommandHandler>>();

        _handler = new ReturnBookCommandHandler(
            _rentalRepositoryMock.Object,
            _bookRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fineCalculatorMock.Object,
            _activityLoggerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRentalDoesNotExist_ShouldThrowRentalNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var rentalId = Guid.NewGuid();
        var command = new ReturnBookCommand(userId, rentalId);

        _rentalRepositoryMock
            .Setup(x => x.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rental?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RentalNotFoundException>()
            .WithMessage($"*{rentalId}*");
    }

    [Fact]
    public async Task Handle_WhenRentalBelongsToAnotherUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var rentalId = Guid.NewGuid();
        var command = new ReturnBookCommand(userId, rentalId);

        var rental = new Rental
        {
            Id = rentalId,
            UserId = otherUserId,
            BookId = Guid.NewGuid(),
            Status = RentalStatus.Active
        };

        _rentalRepositoryMock
            .Setup(x => x.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenRentalAlreadyReturned_ShouldThrowInvalidOperationDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var rentalId = Guid.NewGuid();
        var command = new ReturnBookCommand(userId, rentalId);

        var rental = new Rental
        {
            Id = rentalId,
            UserId = userId,
            BookId = Guid.NewGuid(),
            Status = RentalStatus.Returned
        };

        _rentalRepositoryMock
            .Setup(x => x.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationDomainException>()
            .WithMessage("*already been returned*");
    }

    [Fact]
    public async Task Handle_WhenReturnedOnTime_ShouldAward10PointsAndNoFine()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var rentalId = Guid.NewGuid();
        var command = new ReturnBookCommand(userId, rentalId);

        var rental = new Rental
        {
            Id = rentalId,
            UserId = userId,
            BookId = bookId,
            RentalDate = DateTime.UtcNow.AddDays(-10),
            DueDate = DateTime.UtcNow.AddDays(20),
            Status = RentalStatus.Active
        };

        var book = new Book
        {
            Id = bookId,
            Title = "Test book",
            AvailableCopies = 4,
            TotalCopies = 5
        };

        var user = new User
        {
            Id = userId,
            Points = 0,
            TotalFines = 0
        };

        _rentalRepositoryMock
            .Setup(x => x.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _fineCalculatorMock
            .Setup(x => x.CalculateFine(It.IsAny<Rental>()))
            .Returns(0m);

        _fineCalculatorMock
            .Setup(x => x.CalculatePoints(It.IsAny<Rental>()))
            .Returns(10);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FineAmount.Should().Be(0);
        result.PointsEarned.Should().Be(10);
        result.IsOverdue.Should().BeFalse();
        result.TotalUserPoints.Should().Be(10);
        result.TotalUserFines.Should().Be(0);

        book.AvailableCopies.Should().Be(5);
        user.Points.Should().Be(10);
        rental.Status.Should().Be(RentalStatus.Returned);
        rental.ReturnDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenReturnedLate_ShouldApplyFineAndAward5Points()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var rentalId = Guid.NewGuid();
        var command = new ReturnBookCommand(userId, rentalId);

        var rental = new Rental
        {
            Id = rentalId,
            UserId = userId,
            BookId = bookId,
            RentalDate = DateTime.UtcNow.AddDays(-40),
            DueDate = DateTime.UtcNow.AddDays(-5),
            Status = RentalStatus.Active
        };

        var book = new Book
        {
            Id = bookId,
            Title = "Test book",
            AvailableCopies = 4,
            TotalCopies = 5
        };

        var user = new User
        {
            Id = userId,
            Points = 0,
            TotalFines = 0
        };

        _rentalRepositoryMock
            .Setup(x => x.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _fineCalculatorMock
            .Setup(x => x.CalculateFine(It.IsAny<Rental>()))
            .Returns(5.0m);

        _fineCalculatorMock
            .Setup(x => x.CalculatePoints(It.IsAny<Rental>()))
            .Returns(5);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FineAmount.Should().Be(5.0m);
        result.PointsEarned.Should().Be(5);
        result.IsOverdue.Should().BeTrue();
        result.DaysOverdue.Should().BeGreaterThanOrEqualTo(5);
        result.TotalUserPoints.Should().Be(5);
        result.TotalUserFines.Should().Be(5.0m);

        user.TotalFines.Should().Be(5.0m);
        book.AvailableCopies.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesOnce()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var rentalId = Guid.NewGuid();
        var command = new ReturnBookCommand(userId, rentalId);

        var rental = new Rental
        {
            Id = rentalId,
            UserId = userId,
            BookId = Guid.NewGuid(),
            DueDate = DateTime.UtcNow.AddDays(10),
            Status = RentalStatus.Active
        };

        _rentalRepositoryMock
            .Setup(x => x.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(rental.BookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Book { Id = rental.BookId, Title = "Test" });

        _fineCalculatorMock
            .Setup(x => x.CalculateFine(It.IsAny<Rental>()))
            .Returns(0m);

        _fineCalculatorMock
            .Setup(x => x.CalculatePoints(It.IsAny<Rental>()))
            .Returns(10);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}