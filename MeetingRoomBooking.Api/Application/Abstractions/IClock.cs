namespace MeetingRoomBooking.Api.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
