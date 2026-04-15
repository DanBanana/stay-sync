using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaySync.Application.Features.Invites.Commands;
using StaySync.Application.Features.Invites.Queries;

namespace StaySync.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateInviteCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetInvitesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new RevokeInviteCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("validate")]
    [AllowAnonymous]
    public async Task<IActionResult> Validate([FromQuery] string token, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ValidateInviteQuery(token), cancellationToken);
        return Ok(result);
    }

    [HttpPost("accept")]
    [AllowAnonymous]
    public async Task<IActionResult> Accept([FromBody] AcceptInviteCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
