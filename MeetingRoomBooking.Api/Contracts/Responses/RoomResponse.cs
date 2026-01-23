namespace MeetingRoomBooking.Api.Contracts.Responses;

public sealed record RoomResponse(
    Guid Id,
    string Name,
    string Location,
    int Capacity
);
