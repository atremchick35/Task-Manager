using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

public sealed record TaskListQuery(string? Search, TaskItemStatus? Status, int Page, int PageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public TaskListQuery Normalize() => this with
    {
        Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
        Page = Math.Max(1, Page),
        PageSize = PageSize <= 0 ? DefaultPageSize : Math.Min(PageSize, MaxPageSize)
    };
}

public sealed record TaskListItemDto(
    Guid Id,
    string Title,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateOnly? DueDate,
    DateTimeOffset CreatedAt);

public sealed record TaskDetailsDto(
    Guid Id,
    string Title,
    string Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateOnly? DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record SaveTaskRequest(
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateOnly? DueDate);

public sealed record ChangeTaskStatusRequest(TaskItemStatus Status);
