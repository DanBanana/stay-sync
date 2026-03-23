using MediatR;
using StaySync.Application.Features.ExternalCalendars.DTOs;

namespace StaySync.Application.Features.ExternalCalendars.Queries;

public record GetCalendarByIdQuery(Guid Id) : IRequest<ExternalCalendarDto>;
