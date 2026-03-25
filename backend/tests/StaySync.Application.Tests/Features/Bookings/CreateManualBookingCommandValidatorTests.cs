using FluentAssertions;
using StaySync.Application.Features.Bookings.Commands;

namespace StaySync.Application.Tests.Features.Bookings;

public class CreateManualBookingCommandValidatorTests
{
    private readonly CreateManualBookingCommandValidator _validator = new();

    [Fact]
    public void Validate_CheckOutAfterCheckIn_Passes()
    {
        var command = new CreateManualBookingCommand(
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
        var command = new CreateManualBookingCommand(
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
        var command = new CreateManualBookingCommand(
            Guid.NewGuid(),
            new DateOnly(2026, 4, 10),
            new DateOnly(2026, 4, 5),
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.CheckOut));
    }

    [Fact]
    public void Validate_EmptyRoomId_Fails()
    {
        var command = new CreateManualBookingCommand(
            Guid.Empty,
            new DateOnly(2026, 4, 5),
            new DateOnly(2026, 4, 10),
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.RoomId));
    }
}
