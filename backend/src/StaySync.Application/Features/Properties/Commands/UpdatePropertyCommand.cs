using MediatR;
using StaySync.Application.Features.Properties.DTOs;

namespace StaySync.Application.Features.Properties.Commands;

public record UpdatePropertyCommand(Guid Id, string Name, string? Address) : IRequest<PropertyDto>;
