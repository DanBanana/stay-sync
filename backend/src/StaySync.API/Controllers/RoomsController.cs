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
    [HttpGet("by-property/{propertyId:guid}")]
    public async Task<IActionResult> GetByProperty(Guid propertyId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRoomsByPropertyQuery(propertyId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "PropertyManager")]
    public async Task<IActionResult> Create([FromBody] CreateRoomCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Created($"/api/rooms/{result.Id}", result);
    }
}
