using FluentAssertions;
using StaySync.Application.Features.Bookings.Commands;

namespace StaySync.Application.Tests.Features.Bookings;

public class UpdateManualBookingCommandValidatorTests
{
    private readonly UpdateManualBookingCommandValidator _validator = new();

    [Fact]
    public void Validate_CheckOutAfterCheckIn_Passes()
    {
        var command = new UpdateManualBookingCommand(
            Guid.NewGuid(),
            new DateOnly(2026, 4, 5),
            new DateOnly(2026, 4, 10),
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_CheckOutSameDayAsCheckIn_Fails()
    {
        var command = new UpdateManualBookingCommand(
            Guid.NewGuid(),
            new DateOnly(2026, 4, 5),
            new DateOnly(2026, 4, 5),
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.CheckOut));
    }

    [Fact]
    public void Validate_CheckOutBeforeCheckIn_Fails()
    {
        var command = new UpdateManualBookingCommand(
            Guid.NewGuid(),
            new DateOnly(2026, 4, 10),
            new DateOnly(2026, 4, 5),
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.CheckOut));
    }
}
