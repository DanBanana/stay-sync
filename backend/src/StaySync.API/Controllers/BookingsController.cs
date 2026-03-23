using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaySync.Application.Features.Bookings.Queries;

namespace StaySync.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController(IMediator mediator) : ControllerBase
{
    [HttpGet("by-room/{roomId:guid}")]
    public async Task<IActionResult> GetByRoom(Guid roomId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBookingsByRoomQuery(roomId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-property/{propertyId:guid}")]
    public async Task<IActionResult> GetByProperty(Guid propertyId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBookingsByPropertyQuery(propertyId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBookingByIdQuery(id), cancellationToken);
        return Ok(result);
    }
}
