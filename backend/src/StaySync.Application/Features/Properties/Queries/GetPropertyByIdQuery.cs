using MediatR;
using StaySync.Application.Features.Properties.DTOs;

namespace StaySync.Application.Features.Properties.Queries;

public record GetPropertyByIdQuery(Guid Id) : IRequest<PropertyDto>;
