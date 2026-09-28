using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Application.Tasks;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Providers;

internal sealed class TaskQueryProvider(AppDbContext db) : ITaskQueryProvider
{
    public async Task<PagedResult<TaskListItemDto>> ListAsync(Guid userId, TaskListQuery query, CancellationToken cancellationToken)
    {
        query = query.Normalize();

        var tasks = db.Tasks.AsNoTracking().Where(t => t.UserId == userId);

        if (query.Search is not null)
        {
            var pattern = $"%{EscapeLikePattern(query.Search)}%";
            tasks = tasks.Where(t => EF.Functions.ILike(t.Title, pattern, "\\"));
        }

        if (query.Status is not null)
            tasks = tasks.Where(t => t.Status == query.Status);

        var totalCount = await tasks.CountAsync(cancellationToken);

        var items = await tasks
            .OrderByDescending(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new TaskListItemDto(t.Id, t.Title, t.Status, t.Priority, t.DueDate, t.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskListItemDto>(items, query.Page, query.PageSize, totalCount);
    }

    public Task<TaskDetailsDto?> GetAsync(Guid userId, Guid taskId, CancellationToken cancellationToken) =>
        db.Tasks.AsNoTracking()
            .Where(t => t.Id == taskId && t.UserId == userId)
            .Select(t => new TaskDetailsDto(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.DueDate,
                t.CreatedAt,
                t.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

    private static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
