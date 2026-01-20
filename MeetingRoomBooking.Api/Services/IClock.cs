namespace MeetingRoomBooking.Api.Services;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
