using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Abstractions;

public interface ITaskRepository
{
    Task<TaskItem?> GetOwnedAsync(Guid taskId, Guid userId, CancellationToken cancellationToken);
    void Add(TaskItem task);
    void Remove(TaskItem task);
}
