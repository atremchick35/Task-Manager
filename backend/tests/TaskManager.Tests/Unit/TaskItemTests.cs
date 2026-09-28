using TaskManager.Domain.Common;
using TaskManager.Domain.Tasks;

namespace TaskManager.Tests.Unit;

public sealed class TaskItemTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_TrimsTextAndSetsTimestamps()
    {
        var task = TaskItem.Create(Guid.NewGuid(), "  Write report  ", "  details ", TaskItemStatus.Todo, TaskPriority.High, new DateOnly(2026, 10, 1), Now);

        Assert.Equal("Write report", task.Title);
        Assert.Equal("details", task.Description);
        Assert.Equal(Now, task.CreatedAt);
        Assert.Equal(Now, task.UpdatedAt);
        Assert.NotEqual(Guid.Empty, task.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutTitle_Throws(string? title)
    {
        Assert.Throws<DomainException>(() =>
            TaskItem.Create(Guid.NewGuid(), title!, null, TaskItemStatus.Todo, TaskPriority.Low, null, Now));
    }

    [Fact]
    public void Create_WithTooLongTitle_Throws()
    {
        var title = new string('a', TaskItem.TitleMaxLength + 1);

        Assert.Throws<DomainException>(() =>
            TaskItem.Create(Guid.NewGuid(), title, null, TaskItemStatus.Todo, TaskPriority.Low, null, Now));
    }

    [Fact]
    public void Create_WithoutOwner_Throws()
    {
        Assert.Throws<DomainException>(() =>
            TaskItem.Create(Guid.Empty, "Title", null, TaskItemStatus.Todo, TaskPriority.Low, null, Now));
    }

    [Fact]
    public void ChangeStatus_UpdatesStatusAndTimestamp()
    {
        var task = TaskItem.Create(Guid.NewGuid(), "Title", null, TaskItemStatus.Todo, TaskPriority.Low, null, Now);
        var later = Now.AddHours(1);

        task.ChangeStatus(TaskItemStatus.Done, later);

        Assert.Equal(TaskItemStatus.Done, task.Status);
        Assert.Equal(later, task.UpdatedAt);
        Assert.Equal(Now, task.CreatedAt);
    }

    [Fact]
    public void ChangeStatus_WithUndefinedValue_Throws()
    {
        var task = TaskItem.Create(Guid.NewGuid(), "Title", null, TaskItemStatus.Todo, TaskPriority.Low, null, Now);

        Assert.Throws<DomainException>(() => task.ChangeStatus((TaskItemStatus)42, Now));
    }
}
