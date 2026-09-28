using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Application.Users;
using TaskManager.Domain.Common;
using TaskManager.Domain.Users;

namespace TaskManager.Application.Auth;

internal sealed class AuthService(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer tokenIssuer,
    ITokenRevocationStore revocationStore,
    TimeProvider timeProvider) : IAuthService
{
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var password = request.Password ?? string.Empty;
        if (password.Length is < PasswordMinLength or > PasswordMaxLength)
            throw new ValidationException(
                $"Password must be between {PasswordMinLength} and {PasswordMaxLength} characters.");

        var email = User.NormalizeEmail(request.Email);
        if (await users.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException("User with this email already exists.");

        var user = User.Register(request.Name, email, passwordHasher.Hash(password), timeProvider.GetUtcNow());
        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        string email;
        try
        {
            email = User.NormalizeEmail(request.Email);
        }
        catch (DomainException)
        {
            throw InvalidCredentials();
        }

        var user = await users.GetByEmailAsync(email, cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password ?? string.Empty, user.PasswordHash))
            throw InvalidCredentials();

        return CreateResponse(user);
    }

    public Task LogoutAsync(string tokenId, DateTimeOffset expiresAt, CancellationToken cancellationToken) =>
        revocationStore.RevokeAsync(tokenId, expiresAt, cancellationToken);

    private AuthResponse CreateResponse(User user)
    {
        var token = tokenIssuer.Issue(user);
        return new AuthResponse(token.Value, token.ExpiresAt, new UserDto(user.Id, user.Name, user.Email, user.CreatedAt));
    }

    private static AuthenticationFailedException InvalidCredentials() => new("Invalid email or password.");
}
