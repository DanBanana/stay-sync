namespace StaySync.Domain.Exceptions;

public class ConflictDetectedException(Guid roomId, DateOnly checkIn, DateOnly checkOut)
    : DomainException($"A booking conflict was detected for room {roomId} between {checkIn} and {checkOut}.");
