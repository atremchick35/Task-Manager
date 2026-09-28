using TaskManager.Application.Common;
using TaskManager.Application.Tasks;

namespace TaskManager.Application.Abstractions;

public interface ITaskQueryProvider
{
    Task<PagedResult<TaskListItemDto>> ListAsync(Guid userId, TaskListQuery query, CancellationToken cancellationToken);
    Task<TaskDetailsDto?> GetAsync(Guid userId, Guid taskId, CancellationToken cancellationToken);
}
