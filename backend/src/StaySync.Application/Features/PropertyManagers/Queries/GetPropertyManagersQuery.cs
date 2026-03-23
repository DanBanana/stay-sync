using MediatR;
using StaySync.Application.Features.PropertyManagers.DTOs;

namespace StaySync.Application.Features.PropertyManagers.Queries;

public record GetPropertyManagersQuery : IRequest<List<PropertyManagerDto>>;
