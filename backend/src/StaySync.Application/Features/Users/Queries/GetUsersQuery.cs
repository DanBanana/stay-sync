using MediatR;
using StaySync.Application.Features.Users.DTOs;

namespace StaySync.Application.Features.Users.Queries;

public record GetUsersQuery : IRequest<List<UserDto>>;
