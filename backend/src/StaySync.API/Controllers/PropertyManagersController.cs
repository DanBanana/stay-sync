using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaySync.Application.Features.PropertyManagers.Queries;

namespace StaySync.API.Controllers;

[ApiController]
[Route("api/property-managers")]
[Authorize(Roles = "SuperAdmin")]
public class PropertyManagersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPropertyManagersQuery(), cancellationToken);
        return Ok(result);
    }
}
