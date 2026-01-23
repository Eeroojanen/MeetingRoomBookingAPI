namespace MeetingRoomBooking.Api.Contracts.Responses;

public sealed record ReservationResponse(
    Guid Id,
    Guid RoomId,
    string Title,
    string Organizer,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    DateTimeOffset CreatedAtUtc
);
