using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Users;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Providers;

internal sealed class UserQueryProvider(AppDbContext db) : IUserQueryProvider
{
    public Task<UserDto?> GetAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserDto(u.Id, u.Name, u.Email, u.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);
}
