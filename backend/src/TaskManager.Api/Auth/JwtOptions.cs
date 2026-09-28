using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = null!;

    [Required]
    public string Audience { get; init; } = null!;

    [Required]
    [MinLength(32)]
    public string Secret { get; init; } = null!;

    [Range(1, 24 * 60)]
    public int LifetimeMinutes { get; init; } = 60;
}
