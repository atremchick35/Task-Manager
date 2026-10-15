using Microsoft.Extensions.Logging;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

internal static partial class TaskLog
{
    [LoggerMessage(EventId = 2001, EventName = "task.created", Level = LogLevel.Information,
        Message = "User {UserId} created task {TaskId} with status {TaskStatus} and priority {TaskPriority}")]
    public static partial void TaskCreated(this ILogger logger, Guid userId, Guid taskId, TaskItemStatus taskStatus, TaskPriority taskPriority);

    [LoggerMessage(EventId = 2002, EventName = "task.updated", Level = LogLevel.Information,
        Message = "User {UserId} updated task {TaskId}")]
    public static partial void TaskUpdated(this ILogger logger, Guid userId, Guid taskId);

    [LoggerMessage(EventId = 2003, EventName = "task.status_changed", Level = LogLevel.Information,
        Message = "User {UserId} changed status of task {TaskId} from {PreviousStatus} to {TaskStatus}")]
    public static partial void TaskStatusChanged(this ILogger logger, Guid userId, Guid taskId, TaskItemStatus previousStatus, TaskItemStatus taskStatus);

    [LoggerMessage(EventId = 2004, EventName = "task.deleted", Level = LogLevel.Information,
        Message = "User {UserId} deleted task {TaskId}")]
    public static partial void TaskDeleted(this ILogger logger, Guid userId, Guid taskId);

    [LoggerMessage(EventId = 2005, EventName = "task.not_found", Level = LogLevel.Warning,
        Message = "Task {TaskId} not found for user {UserId}")]
    public static partial void TaskNotFound(this ILogger logger, Guid userId, Guid taskId);
}
