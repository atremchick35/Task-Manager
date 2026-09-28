namespace TaskManager.Application.Users;

public sealed record UserDto(Guid Id, string Name, string Email, DateTimeOffset CreatedAt);
