namespace MeetingRoomBooking.Api.Contracts.Requests;

public sealed record CreateReservationRequest(
    string Title,
    string Organizer,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc
);
