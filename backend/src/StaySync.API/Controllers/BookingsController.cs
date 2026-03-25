using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaySync.Application.Features.Bookings.Commands;
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

    [HttpGet("calendar")]
    public async Task<IActionResult> GetForCalendar(
        [FromQuery] Guid propertyId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetBookingsForCalendarQuery(propertyId, from, to), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateManual([FromBody] CreateManualBookingCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateManual(Guid id, [FromBody] UpdateManualBookingRequest body, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateManualBookingCommand(id, body.CheckIn, body.CheckOut, body.GuestName), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteManual(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteManualBookingCommand(id), cancellationToken);
        return NoContent();
    }
}

public record UpdateManualBookingRequest(DateOnly CheckIn, DateOnly CheckOut, string? GuestName);
