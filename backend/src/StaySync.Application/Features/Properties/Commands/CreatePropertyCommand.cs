using MediatR;
using StaySync.Application.Features.Properties.DTOs;

namespace StaySync.Application.Features.Properties.Commands;

public record CreatePropertyCommand(string Name, string? Address, Guid? PropertyManagerId = null) : IRequest<PropertyDto>;
