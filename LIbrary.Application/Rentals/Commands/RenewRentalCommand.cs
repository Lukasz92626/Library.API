using Library.Application.DTOs.Rentals;
using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Library.Application.Rentals.Commands;

public record RenewRentalCommand(Guid UserId, Guid RentalId) : IRequest<RenewRentalResponse>;

public class RenewRentalCommandHandler : IRequestHandler<RenewRentalCommand, RenewRentalResponse>
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RenewRentalCommandHandler> _logger;

    private const int ExtensionDays = 14;
    private const int MaxRenewals = 2;

    public RenewRentalCommandHandler(
        IRentalRepository rentalRepository,
        IUnitOfWork unitOfWork,
        ILogger<RenewRentalCommandHandler> logger)
    {
        _rentalRepository = rentalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RenewRentalResponse> Handle(RenewRentalCommand request, CancellationToken cancellationToken)
    {
        var rental = await _rentalRepository.GetByIdAsync(request.RentalId, cancellationToken);
        if (rental == null)
            throw new RentalNotFoundException(request.RentalId);

        // Checks the rental
        if (rental.UserId != request.UserId)
            throw new UnauthorizedAccessException("You can only renew your own rentals.");
        if (rental.Status != RentalStatus.Active)
            throw new InvalidOperationDomainException("Only active rentals can be renewed.");
        if (rental.RenewalCount >= MaxRenewals)
            throw new InvalidOperationDomainException($"Maximum number of renewals ({MaxRenewals}) reached.");
        if (rental.DueDate < DateTime.UtcNow)
            throw new InvalidOperationDomainException("Cannot renew an overdue rental.");
        
        rental.DueDate = rental.DueDate.AddDays(ExtensionDays);
        rental.RenewalCount++;

        // Save changes
        await _rentalRepository.UpdateAsync(rental, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation(
            "User {UserId} renewed rental {RentalId}. Ne due date: {DueDate}, Renewal count: {RenewalCount}",
            request.UserId, rental.Id, rental.DueDate, rental.RenewalCount);

        return new RenewRentalResponse
        {
            RentalId = rental.Id,
            BookTitle = rental.Book?.Title ?? "Unknown",
            NewDueDate = rental.DueDate,
            RenewalCount = rental.RenewalCount,
            MaxRenewals = MaxRenewals,
            DaysExtended = ExtensionDays
        };
    }
}