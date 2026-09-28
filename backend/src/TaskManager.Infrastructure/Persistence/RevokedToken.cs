namespace TaskManager.Infrastructure.Persistence;

public sealed class RevokedToken
{
    public const int TokenIdMaxLength = 64;

    public required string TokenId { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}
