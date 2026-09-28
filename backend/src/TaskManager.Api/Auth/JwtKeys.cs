using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TaskManager.Api.Auth;

internal static class JwtKeys
{
    public static SymmetricSecurityKey CreateSigningKey(string secret) => new(Encoding.UTF8.GetBytes(secret));
}
