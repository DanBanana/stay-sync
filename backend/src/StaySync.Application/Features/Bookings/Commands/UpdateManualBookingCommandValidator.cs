using FluentValidation;

namespace StaySync.Application.Features.Bookings.Commands;

public class UpdateManualBookingCommandValidator : AbstractValidator<UpdateManualBookingCommand>
{
    public UpdateManualBookingCommandValidator()
    {
        RuleFor(x => x.CheckOut)
            .GreaterThan(x => x.CheckIn)
            .WithMessage("Check-out must be after check-in.");
    }
}
