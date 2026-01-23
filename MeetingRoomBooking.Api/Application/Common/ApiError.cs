namespace MeetingRoomBooking.Api.Application.Common;

public sealed record ApiError(string Title, string Detail, int StatusCode);
