using System.Text.Json;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.Services;

public class ActivityLogger : IActivityLogger
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivityLogger> _logger;

    public ActivityLogger(
        IActivityLogRepository activityLogRepository,
        IUnitOfWork unitOfWork,
        ILogger<ActivityLogger> logger)
    {
        _activityLogRepository = activityLogRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task LogAsync(Guid userId, string action, object? metadata = null, CancellationToken cancellationToken = default)
    {
        var log = new UserActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null
        };

        await _activityLogRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activity logged: User {UserId}, {Action}", userId, action);
    }
}