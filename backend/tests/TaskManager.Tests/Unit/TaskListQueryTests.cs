using TaskManager.Application.Tasks;

namespace TaskManager.Tests.Unit;

public sealed class TaskListQueryTests
{
    [Theory]
    [InlineData(0, 0, 1, TaskListQuery.DefaultPageSize)]
    [InlineData(-5, 10, 1, 10)]
    [InlineData(3, 1000, 3, TaskListQuery.MaxPageSize)]
    public void Normalize_ClampsPaging(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        var query = new TaskListQuery(null, null, page, pageSize).Normalize();

        Assert.Equal(expectedPage, query.Page);
        Assert.Equal(expectedPageSize, query.PageSize);
    }

    [Theory]
    [InlineData("   ", null)]
    [InlineData("  report ", "report")]
    public void Normalize_TrimsSearch(string search, string? expected)
    {
        Assert.Equal(expected, new TaskListQuery(search, null, 1, 10).Normalize().Search);
    }
}
