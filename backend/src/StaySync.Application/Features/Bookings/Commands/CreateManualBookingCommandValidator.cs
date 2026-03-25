using FluentValidation;

namespace StaySync.Application.Features.Bookings.Commands;

public class CreateManualBookingCommandValidator : AbstractValidator<CreateManualBookingCommand>
{
    public CreateManualBookingCommandValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.CheckOut)
            .GreaterThan(x => x.CheckIn)
            .WithMessage("Check-out must be after check-in.");
    }
}
