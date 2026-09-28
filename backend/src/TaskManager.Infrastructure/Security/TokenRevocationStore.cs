using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Security;

internal sealed class TokenRevocationStore(AppDbContext db) : ITokenRevocationStore
{
    public async Task RevokeAsync(string tokenId, DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO revoked_tokens (token_id, expires_at) VALUES ({tokenId}, {expiresAt}) ON CONFLICT (token_id) DO NOTHING",
            cancellationToken);
    }

    public Task<bool> IsRevokedAsync(string tokenId, CancellationToken cancellationToken) =>
        db.RevokedTokens.AsNoTracking().AnyAsync(t => t.TokenId == tokenId, cancellationToken);

    public Task<int> PurgeExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) =>
        db.RevokedTokens.Where(t => t.ExpiresAt < now).ExecuteDeleteAsync(cancellationToken);
}
