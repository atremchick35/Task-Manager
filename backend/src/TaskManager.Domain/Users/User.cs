using System.Net.Mail;
using TaskManager.Domain.Common;

namespace TaskManager.Domain.Users;

public sealed class User
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 256;

    private User()
    {
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    public static User Register(string name, string email, string passwordHash, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        return new User
        {
            Id = Guid.NewGuid(),
            Name = Guard.RequiredText(name, NameMaxLength, "Name"),
            Email = NormalizeEmail(email),
            PasswordHash = passwordHash,
            CreatedAt = now
        };
    }

    public static string NormalizeEmail(string? email)
    {
        var normalized = Guard.RequiredText(email, EmailMaxLength, "Email").ToLowerInvariant();
        if (!MailAddress.TryCreate(normalized, out var address) || address.Address != normalized)
            throw new DomainException("Email has invalid format.");

        return normalized;
    }
}
