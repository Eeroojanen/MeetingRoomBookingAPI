namespace MeetingRoomBooking.Api.Contracts;

public sealed record ReservationResponse(
    Guid Id,
    Guid RoomId,
    string Title,
    string Organizer,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    DateTimeOffset CreatedAtUtc
);
