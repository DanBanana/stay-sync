using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.PropertyManagers.DTOs;

namespace StaySync.Application.Features.PropertyManagers.Queries;

public class GetPropertyManagersQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetPropertyManagersQuery, List<PropertyManagerDto>>
{
    public async Task<List<PropertyManagerDto>> Handle(GetPropertyManagersQuery request, CancellationToken cancellationToken)
    {
        return await context.PropertyManagers
            .OrderBy(pm => pm.DisplayName)
            .Select(pm => new PropertyManagerDto(pm.Id, pm.DisplayName))
            .ToListAsync(cancellationToken);
    }
}
