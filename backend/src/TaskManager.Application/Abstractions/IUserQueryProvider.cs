using TaskManager.Application.Users;

namespace TaskManager.Application.Abstractions;

public interface IUserQueryProvider
{
    Task<UserDto?> GetAsync(Guid userId, CancellationToken cancellationToken);
}
