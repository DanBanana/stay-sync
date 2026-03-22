using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Properties.DTOs;

namespace StaySync.Application.Features.Properties.Queries;

public class GetPropertyByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<GetPropertyByIdQuery, PropertyDto>
{
    public async Task<PropertyDto> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Property", request.Id);

        if (currentUser.Role != "SuperAdmin" && property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        return new PropertyDto(property.Id, property.Name, property.Address, property.PropertyManagerId, property.CreatedAt);
    }
}
