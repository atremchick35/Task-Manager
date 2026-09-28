using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace TaskManager.Api.Auth;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id)
            ? id
            : throw new InvalidOperationException("Authenticated principal has no valid subject claim.");

    public static string GetTokenId(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(JwtRegisteredClaimNames.Jti)
        ?? throw new InvalidOperationException("Authenticated principal has no token id claim.");

    public static DateTimeOffset GetTokenExpiration(this ClaimsPrincipal principal) =>
        long.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Exp), out var exp)
            ? DateTimeOffset.FromUnixTimeSeconds(exp)
            : throw new InvalidOperationException("Authenticated principal has no expiration claim.");
}
