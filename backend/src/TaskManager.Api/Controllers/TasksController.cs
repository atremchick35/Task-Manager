using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Auth;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Application.Tasks;
using TaskManager.Domain.Tasks;

namespace TaskManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController(ITaskService taskService, ITaskQueryProvider taskQueries) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<TaskListItemDto>> List(
        [FromQuery] string? search,
        [FromQuery] TaskItemStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = TaskListQuery.DefaultPageSize,
        CancellationToken cancellationToken = default) =>
        taskQueries.ListAsync(User.GetUserId(), new TaskListQuery(search, status, page, pageSize), cancellationToken);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDetailsDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var task = await taskQueries.GetAsync(User.GetUserId(), id, cancellationToken);
        return task is null ? NotFound() : task;
    }

    [HttpPost]
    [ProducesResponseType<TaskDetailsDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<TaskDetailsDto>> Create(SaveTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await taskService.CreateAsync(User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}")]
    public Task<TaskDetailsDto> Update(Guid id, SaveTaskRequest request, CancellationToken cancellationToken) =>
        taskService.UpdateAsync(User.GetUserId(), id, request, cancellationToken);

    [HttpPatch("{id:guid}/status")]
    public Task<TaskDetailsDto> ChangeStatus(Guid id, ChangeTaskStatusRequest request, CancellationToken cancellationToken) =>
        taskService.ChangeStatusAsync(User.GetUserId(), id, request, cancellationToken);

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await taskService.DeleteAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }
}
