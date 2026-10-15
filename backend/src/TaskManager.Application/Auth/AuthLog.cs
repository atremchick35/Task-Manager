using Microsoft.Extensions.Logging;

namespace TaskManager.Application.Auth;

internal static partial class AuthLog
{
    public const string EmailTaken = "email_taken";
    public const string InvalidPasswordLength = "invalid_password_length";
    public const string InvalidEmail = "invalid_email";
    public const string UnknownEmail = "unknown_email";
    public const string WrongPassword = "wrong_password";

    [LoggerMessage(EventId = 1001, EventName = "user.registered", Level = LogLevel.Information,
        Message = "User {UserId} registered")]
    public static partial void UserRegistered(this ILogger logger, Guid userId);

    [LoggerMessage(EventId = 1002, EventName = "user.registration_rejected", Level = LogLevel.Warning,
        Message = "Registration rejected: {Reason}")]
    public static partial void RegistrationRejected(this ILogger logger, string reason);

    [LoggerMessage(EventId = 1003, EventName = "user.login_succeeded", Level = LogLevel.Information,
        Message = "User {UserId} logged in")]
    public static partial void LoginSucceeded(this ILogger logger, Guid userId);

    [LoggerMessage(EventId = 1004, EventName = "user.login_failed", Level = LogLevel.Warning,
        Message = "Login failed for user {UserId}: {Reason}")]
    public static partial void LoginFailed(this ILogger logger, Guid? userId, string reason);

    [LoggerMessage(EventId = 1005, EventName = "user.logged_out", Level = LogLevel.Information,
        Message = "User {UserId} logged out")]
    public static partial void LoggedOut(this ILogger logger, Guid userId);
}
