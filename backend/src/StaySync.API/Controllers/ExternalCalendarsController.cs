using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaySync.Application.Features.ExternalCalendars.Commands;
using StaySync.Application.Features.ExternalCalendars.Queries;

namespace StaySync.API.Controllers;

[ApiController]
[Route("api/external-calendars")]
[Authorize]
public class ExternalCalendarsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCalendarByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-room/{roomId:guid}")]
    public async Task<IActionResult> GetByRoom(Guid roomId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetExternalCalendarsByRoomQuery(roomId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,PropertyManager")]
    public async Task<IActionResult> Create([FromBody] CreateExternalCalendarCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Created($"/api/external-calendars/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,PropertyManager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExternalCalendarCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,PropertyManager")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteExternalCalendarCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/sync")]
    [Authorize(Roles = "SuperAdmin,PropertyManager")]
    public async Task<IActionResult> Sync(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SyncCalendarCommand(id), cancellationToken);
        return Ok(result);
    }
}
