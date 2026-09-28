namespace TaskManager.Application.Tasks;

public interface ITaskService
{
    Task<TaskDetailsDto> CreateAsync(Guid userId, SaveTaskRequest request, CancellationToken cancellationToken);
    Task<TaskDetailsDto> UpdateAsync(Guid userId, Guid taskId, SaveTaskRequest request, CancellationToken cancellationToken);
    Task<TaskDetailsDto> ChangeStatusAsync(Guid userId, Guid taskId, ChangeTaskStatusRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken);
}
