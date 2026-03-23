using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaySync.Application.Features.Rooms.Commands;
using StaySync.Application.Features.Rooms.Queries;

namespace StaySync.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRoomByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-property/{propertyId:guid}")]
    public async Task<IActionResult> GetByProperty(Guid propertyId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRoomsByPropertyQuery(propertyId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,PropertyManager")]
    public async Task<IActionResult> Create([FromBody] CreateRoomCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Created($"/api/rooms/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,PropertyManager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,PropertyManager")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteRoomCommand(id), cancellationToken);
        return NoContent();
    }
}
