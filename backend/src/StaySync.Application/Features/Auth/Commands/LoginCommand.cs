using MediatR;
using StaySync.Application.Features.Auth.DTOs;

namespace StaySync.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResultDto>;
