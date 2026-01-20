namespace MeetingRoomBooking.Api.Contracts;

public sealed record RoomResponse(
    Guid Id,
    string Name,
    string Location,
    int Capacity
);
