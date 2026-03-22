using FluentValidation;

namespace StaySync.Application.Features.Rooms.Commands;

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}
