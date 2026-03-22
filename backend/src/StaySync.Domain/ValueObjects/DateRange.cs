namespace StaySync.Domain.ValueObjects;

public record DateRange(DateOnly CheckIn, DateOnly CheckOut)
{
    public bool Overlaps(DateRange other) =>
        CheckIn < other.CheckOut && other.CheckIn < CheckOut;
}
