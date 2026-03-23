using MediatR;

namespace StaySync.Application.Features.Properties.Commands;

public record DeletePropertyCommand(Guid Id) : IRequest;
