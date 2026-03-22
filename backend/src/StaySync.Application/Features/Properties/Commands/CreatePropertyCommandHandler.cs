using MediatR;
using StaySync.Application.Common.Exceptions;
using StaySync.Application.Common.Interfaces;
using StaySync.Application.Features.Properties.DTOs;
using StaySync.Domain.Entities;

namespace StaySync.Application.Features.Properties.Commands;

public class CreatePropertyCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser) : IRequestHandler<CreatePropertyCommand, PropertyDto>
{
    public async Task<PropertyDto> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.PropertyManagerId is null)
            throw new ForbiddenException();

        var property = new Property
        {
            Name = request.Name,
            Address = request.Address,
            PropertyManagerId = currentUser.PropertyManagerId.Value
        };

        context.Properties.Add(property);
        await context.SaveChangesAsync(cancellationToken);

        return new PropertyDto(property.Id, property.Name, property.Address, property.PropertyManagerId, property.CreatedAt);
    }
}
