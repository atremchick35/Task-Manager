using System.Net;
using System.Net.Http.Json;
using TaskManager.Application.Common;
using TaskManager.Application.Tasks;
using TaskManager.Domain.Tasks;

namespace TaskManager.Tests.Integration;

[Collection(ApiCollection.Name)]
public sealed class TasksApiTests(ApiFactory factory)
{
    [Fact]
    public async Task Crud_Lifecycle_Works()
    {
        var (client, _) = await ApiClient.RegisterAsync(factory);
        var dueDate = new DateOnly(2026, 12, 31);

        var created = await client.PostAsJsonAsync("/api/tasks",
            new SaveTaskRequest("Prepare report", "Quarterly numbers", TaskItemStatus.Todo, TaskPriority.High, dueDate), ApiClient.Json);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var task = await created.ReadAsync<TaskDetailsDto>();
        Assert.Equal(dueDate, task.DueDate);

        var fetched = await (await client.GetAsync($"/api/tasks/{task.Id}")).ReadAsync<TaskDetailsDto>();
        Assert.Equal("Quarterly numbers", fetched.Description);

        var updated = await client.PutAsJsonAsync($"/api/tasks/{task.Id}",
            new SaveTaskRequest("Prepare final report", null, TaskItemStatus.InProgress, TaskPriority.Low, null), ApiClient.Json);
        var updatedTask = await updated.ReadAsync<TaskDetailsDto>();
        Assert.Equal("Prepare final report", updatedTask.Title);
        Assert.Equal(string.Empty, updatedTask.Description);
        Assert.Null(updatedTask.DueDate);

        var statusChanged = await client.PatchAsJsonAsync($"/api/tasks/{task.Id}/status",
            new ChangeTaskStatusRequest(TaskItemStatus.Done), ApiClient.Json);
        Assert.Equal(TaskItemStatus.Done, (await statusChanged.ReadAsync<TaskDetailsDto>()).Status);

        var deleted = await client.DeleteAsync($"/api/tasks/{task.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/tasks/{task.Id}")).StatusCode);
    }

    [Fact]
    public async Task Create_WithoutTitle_ReturnsBadRequest()
    {
        var (client, _) = await ApiClient.RegisterAsync(factory);

        var response = await client.PostAsJsonAsync("/api/tasks",
            new SaveTaskRequest(" ", null, TaskItemStatus.Todo, TaskPriority.Low, null), ApiClient.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithUnknownStatus_ReturnsBadRequest()
    {
        var (client, _) = await ApiClient.RegisterAsync(factory);

        var response = await client.PostAsJsonAsync("/api/tasks",
            new { title = "Task", status = "Unknown", priority = "Low" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Tasks_AreIsolatedBetweenUsers()
    {
        var (owner, _) = await ApiClient.RegisterAsync(factory);
        var (stranger, _) = await ApiClient.RegisterAsync(factory);
        var task = await CreateAsync(owner, "Private task");

        Assert.Equal(HttpStatusCode.NotFound, (await stranger.GetAsync($"/api/tasks/{task.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await stranger.DeleteAsync($"/api/tasks/{task.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await stranger.PatchAsJsonAsync($"/api/tasks/{task.Id}/status",
            new ChangeTaskStatusRequest(TaskItemStatus.Done), ApiClient.Json)).StatusCode);

        var strangerList = await (await stranger.GetAsync("/api/tasks")).ReadAsync<PagedResult<TaskListItemDto>>();
        Assert.Equal(0, strangerList.TotalCount);
    }

    [Fact]
    public async Task List_SupportsSearchStatusFilterAndPagination()
    {
        var (client, _) = await ApiClient.RegisterAsync(factory);
        await CreateAsync(client, "Alpha one");
        await CreateAsync(client, "alpha two", TaskItemStatus.Done);
        await CreateAsync(client, "ALPHA three");
        await CreateAsync(client, "Beta");
        await CreateAsync(client, "100% done");

        var search = await (await client.GetAsync("/api/tasks?search=alpha&pageSize=2")).ReadAsync<PagedResult<TaskListItemDto>>();
        Assert.Equal(3, search.TotalCount);
        Assert.Equal(2, search.TotalPages);
        Assert.Equal(2, search.Items.Count);
        Assert.Equal("ALPHA three", search.Items[0].Title);

        var secondPage = await (await client.GetAsync("/api/tasks?search=alpha&pageSize=2&page=2")).ReadAsync<PagedResult<TaskListItemDto>>();
        Assert.Equal("Alpha one", Assert.Single(secondPage.Items).Title);

        var byStatus = await (await client.GetAsync("/api/tasks?search=alpha&status=Done")).ReadAsync<PagedResult<TaskListItemDto>>();
        Assert.Equal("alpha two", Assert.Single(byStatus.Items).Title);

        var wildcard = await (await client.GetAsync("/api/tasks?search=%25")).ReadAsync<PagedResult<TaskListItemDto>>();
        Assert.Equal("100% done", Assert.Single(wildcard.Items).Title);
    }

    [Fact]
    public async Task List_CapsPageSize()
    {
        var (client, _) = await ApiClient.RegisterAsync(factory);

        var page = await (await client.GetAsync("/api/tasks?pageSize=100000")).ReadAsync<PagedResult<TaskListItemDto>>();

        Assert.Equal(TaskListQuery.MaxPageSize, page.PageSize);
    }

    private static async Task<TaskDetailsDto> CreateAsync(HttpClient client, string title, TaskItemStatus status = TaskItemStatus.Todo)
    {
        var response = await client.PostAsJsonAsync("/api/tasks",
            new SaveTaskRequest(title, null, status, TaskPriority.Medium, null), ApiClient.Json);
        response.EnsureSuccessStatusCode();
        return await response.ReadAsync<TaskDetailsDto>();
    }
}
