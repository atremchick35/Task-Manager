using Microsoft.Extensions.Logging;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

internal sealed class TaskService(
    ITaskRepository tasks,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<TaskService> logger) : ITaskService
{
    public async Task<TaskDetailsDto> CreateAsync(Guid userId, SaveTaskRequest request, CancellationToken cancellationToken)
    {
        var task = TaskItem.Create(
            userId,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.DueDate,
            timeProvider.GetUtcNow());

        tasks.Add(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.TaskCreated(userId, task.Id, task.Status, task.Priority);
        return ToDetails(task);
    }

    public async Task<TaskDetailsDto> UpdateAsync(Guid userId, Guid taskId, SaveTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await GetOwnedAsync(userId, taskId, cancellationToken);
        task.Update(request.Title, request.Description, request.Status, request.Priority, request.DueDate, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.TaskUpdated(userId, task.Id);
        return ToDetails(task);
    }

    public async Task<TaskDetailsDto> ChangeStatusAsync(Guid userId, Guid taskId, ChangeTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var task = await GetOwnedAsync(userId, taskId, cancellationToken);
        var previousStatus = task.Status;
        task.ChangeStatus(request.Status, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.TaskStatusChanged(userId, task.Id, previousStatus, task.Status);
        return ToDetails(task);
    }

    public async Task DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken)
    {
        var task = await GetOwnedAsync(userId, taskId, cancellationToken);
        tasks.Remove(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.TaskDeleted(userId, task.Id);
    }

    private async Task<TaskItem> GetOwnedAsync(Guid userId, Guid taskId, CancellationToken cancellationToken)
    {
        var task = await tasks.GetOwnedAsync(taskId, userId, cancellationToken);
        if (task is not null)
            return task;

        logger.TaskNotFound(userId, taskId);
        throw new NotFoundException("Task not found.");
    }

    private static TaskDetailsDto ToDetails(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.Priority,
        task.DueDate,
        task.CreatedAt,
        task.UpdatedAt);
}
