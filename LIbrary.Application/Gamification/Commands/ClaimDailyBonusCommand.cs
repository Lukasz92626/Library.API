using Library.Application.DTOs.Gamification;
using Library.Application.Interfaces;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;

using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Library.Application.Gamification.Commands;

public record ClaimDailyBonusCommand(Guid UserId) : IRequest<DailyBonusResponse>;

public class ClaimDailyBonusCommandHandler : IRequestHandler<ClaimDailyBonusCommand, DailyBonusResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly IActivityLogger _activityLogger;
    private readonly ILogger<ClaimDailyBonusCommandHandler> _logger;

    private const string CacheKeyPrefix = "DailyBonus_";
    private const int BonusPoints = 5;

    public ClaimDailyBonusCommandHandler(
        IUserRepository userRepository,
        IRentalRepository rentalRepository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        IActivityLogger activityLogger,
        ILogger<ClaimDailyBonusCommandHandler> logger)
    {
        _userRepository = userRepository;
        _rentalRepository = rentalRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<DailyBonusResponse> Handle(ClaimDailyBonusCommand request, CancellationToken cancellationToken)
    {
        // Checks user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new UserNotFoundException(request.UserId);
        var activeRentals = await _rentalRepository.GetActiveRentalsByUserIdAsync(request.UserId, cancellationToken);
        if (!activeRentals.Any())
        {
            return new DailyBonusResponse
            {
                Success = false,
                Message = "You must have at least one rental.",
                BonusPoints = 0,
                TotalPoints = user.Points,
                NextBonusAvailable = DateTime.UtcNow.Date.AddDays(1)
            };
        }
        
        var cacheKey = $"{CacheKeyPrefix}{request.UserId}";
        var lastClaimDate = _cache.Get<DateTime?>(cacheKey);
        var today = DateTime.UtcNow.Date;
        if (lastClaimDate.HasValue && lastClaimDate.Value.Date == today)
        {
            var nextAvailable = today.AddDays(1);
            return new DailyBonusResponse
            {
                Success = false,
                Message = "You have already claimed your daily bonus today.",
                BonusPoints = 0,
                TotalPoints = user.Points,
                NextBonusAvailable = nextAvailable
            };
        }
        
        user.Points += BonusPoints;
        
        await _activityLogger.LogAsync(request.UserId, "EarnedPoints", new
        {
            Points = BonusPoints,
            Reason = "Daily bonus"
        }, cancellationToken);

        // Saving changes
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _cache.Set(cacheKey, DateTime.UtcNow, DateTimeOffset.UtcNow.Date.AddDays(1));
        _logger.LogInformation("User {UserId} claimed daily bonus. Points: {Points}", request.UserId, user.Points);

        return new DailyBonusResponse
        {
            Success = true,
            Message = "Daily bonus claimed successfully!",
            BonusPoints = BonusPoints,
            TotalPoints = user.Points,
            NextBonusAvailable = DateTime.UtcNow.Date.AddDays(1)
        };
    }
}