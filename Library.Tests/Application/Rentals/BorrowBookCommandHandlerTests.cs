using FluentAssertions;

using Library.Application.Interfaces;
using Library.Application.Rentals.Commands;

using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using AutoMapper;
using Library.Application.Mapping;

namespace Library.Tests.Application.Rentals;

public class BorrowBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IRentalRepository> _rentalRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IActivityLogger> _activityLoggerMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<BorrowBookCommandHandler>> _loggerMock;
    private readonly BorrowBookCommandHandler _handler;

    public BorrowBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _rentalRepositoryMock = new Mock<IRentalRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _activityLoggerMock = new Mock<IActivityLogger>();
        _loggerMock = new Mock<ILogger<BorrowBookCommandHandler>>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<RentalProfile>();
        }, NullLoggerFactory.Instance);
        _mapper = mapperConfig.CreateMapper();

        _handler = new BorrowBookCommandHandler(
            _bookRepositoryMock.Object,
            _rentalRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _activityLoggerMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenBookDoesNotExist_ShouldThrowBookNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var command = new BorrowBookCommand(userId, bookId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Email = "test@test.com", FullName = "Test User" });

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BookNotFoundException>()
            .WithMessage($"*{bookId}*");
    }

    [Fact]
    public async Task Handle_WhenBookHasNoAvailableCopies_ShouldThrowBookUnavailableException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var command = new BorrowBookCommand(userId, bookId);

        var book = new Book
        {
            Id = bookId,
            Title = "Test Book",
            Author = "Test Author",
            ISBN = "1234567890123",
            TotalCopies = 1,
            AvailableCopies = 0 // Brak dostępnych
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Email = "test@test.com", FullName = "Test User" });

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BookUnavailableException>()
            .WithMessage($"*{bookId}*");
    }

    [Fact]
    public async Task Handle_WhenUserHasMaxRentals_ShouldThrowRentalLimitExceededException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var command = new BorrowBookCommand(userId, bookId);

        var book = new Book
        {
            Id = bookId,
            Title = "Test Book",
            Author = "Test Author",
            ISBN = "1234567890123",
            TotalCopies = 5,
            AvailableCopies = 5
        };

        var activeRentals = Enumerable.Range(1, 5).Select(i => new Rental
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BookId = Guid.NewGuid(),
            Status = RentalStatus.Active
        }).ToList();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Email = "test@test.com", FullName = "Test User" });

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _rentalRepositoryMock
            .Setup(x => x.GetActiveRentalsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeRentals);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RentalLimitExceededException>()
            .WithMessage("*5*");
    }

    [Fact]
    public async Task Handle_WhenAllConditionsMet_ShouldCreateRentalAndDecreaseAvailableCopies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var command = new BorrowBookCommand(userId, bookId);

        var book = new Book
        {
            Id = bookId,
            Title = "Test book",
            Author = "Test Author",
            ISBN = "9780132350884",
            TotalCopies = 5,
            AvailableCopies = 5
        };

        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            FullName = "Test User",
            Points = 0
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _rentalRepositoryMock
            .Setup(x => x.GetActiveRentalsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Rental>());

        _rentalRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _bookRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.BookTitle.Should().Be("Test book");
        result.Status.Should().Be("Active");
        book.AvailableCopies.Should().Be(4);

        _rentalRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _bookRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectDueDate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var command = new BorrowBookCommand(userId, bookId);
        var beforeTest = DateTime.UtcNow;

        var book = new Book
        {
            Id = bookId,
            Title = "Test Book",
            Author = "Test Author",
            ISBN = "1234567890123",
            TotalCopies = 5,
            AvailableCopies = 5
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Email = "test@test.com", FullName = "Test User" });

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _rentalRepositoryMock
            .Setup(x => x.GetActiveRentalsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Rental>());

        Rental? capturedRental = null;
        _rentalRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()))
            .Callback<Rental, CancellationToken>((r, ct) => capturedRental = r)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedRental.Should().NotBeNull();
        capturedRental!.DueDate.Should().BeCloseTo(beforeTest.AddDays(30), TimeSpan.FromSeconds(5));
        capturedRental.Status.Should().Be(RentalStatus.Active);
    }
}