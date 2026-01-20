namespace MeetingRoomBooking.Api.Contracts;

public sealed record CreateReservationRequest(
    string Title,
    string Organizer,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc
);
