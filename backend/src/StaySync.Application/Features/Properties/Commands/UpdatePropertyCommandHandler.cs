using MediatR;
using Microsoft.EntityFrameworkCore;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Properties.DTOs;

namespace StaySync.Application.Features.Properties.Commands;

public class UpdatePropertyCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<UpdatePropertyCommand, PropertyDto>
{
    public async Task<PropertyDto> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Property", request.Id);

        if (currentUser.Role != "SuperAdmin" && property.PropertyManagerId != currentUser.PropertyManagerId)
            throw new ForbiddenException();

        property.Name = request.Name;
        property.Address = request.Address;

        await context.SaveChangesAsync(cancellationToken);

        return new PropertyDto(property.Id, property.Name, property.Address, property.PropertyManagerId, property.CreatedAt);
    }
}
