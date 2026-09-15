namespace Library.Application.Interfaces;

public interface IActivityLogger
{
    Task LogAsync(Guid userId, string action, object? metadata = null, CancellationToken cancellationToken = default);
}