using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Properties.DTOs;

namespace StaySync.Application.Features.Properties.Queries;

public class GetPropertiesQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<GetPropertiesQuery, List<PropertyDto>>
{
    public async Task<List<PropertyDto>> Handle(GetPropertiesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Properties.AsQueryable();

        if (currentUser.Role != "SuperAdmin")
            query = query.Where(p => p.PropertyManagerId == currentUser.PropertyManagerId);

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new PropertyDto(p.Id, p.Name, p.Address, p.PropertyManagerId, p.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
