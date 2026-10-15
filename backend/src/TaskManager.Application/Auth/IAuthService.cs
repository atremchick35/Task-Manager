namespace TaskManager.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task LogoutAsync(Guid userId, string tokenId, DateTimeOffset expiresAt, CancellationToken cancellationToken);
}
