using TaskManager.Domain.Common;

namespace TaskManager.Domain.Tasks;

public sealed class TaskItem
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 4000;

    private TaskItem()
    {
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public TaskItemStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static TaskItem Create(
        Guid userId,
        string title,
        string? description,
        TaskItemStatus status,
        TaskPriority priority,
        DateOnly? dueDate,
        DateTimeOffset now)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Task owner is required.");

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = now
        };
        task.Update(title, description, status, priority, dueDate, now);
        return task;
    }

    public void Update(
        string title,
        string? description,
        TaskItemStatus status,
        TaskPriority priority,
        DateOnly? dueDate,
        DateTimeOffset now)
    {
        Title = Guard.RequiredText(title, TitleMaxLength, "Title");
        Description = Guard.OptionalText(description, DescriptionMaxLength, "Description");
        Status = Guard.Defined(status, "Status");
        Priority = Guard.Defined(priority, "Priority");
        DueDate = dueDate;
        UpdatedAt = now;
    }

    public void ChangeStatus(TaskItemStatus status, DateTimeOffset now)
    {
        Status = Guard.Defined(status, "Status");
        UpdatedAt = now;
    }
}
