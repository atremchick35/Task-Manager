namespace TaskManager.Application.Abstractions;

public interface ITokenRevocationStore
{
    Task RevokeAsync(string tokenId, DateTimeOffset expiresAt, CancellationToken cancellationToken);
    Task<bool> IsRevokedAsync(string tokenId, CancellationToken cancellationToken);
    Task<int> PurgeExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken);
}
