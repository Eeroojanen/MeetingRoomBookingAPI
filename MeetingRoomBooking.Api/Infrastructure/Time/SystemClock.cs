using MeetingRoomBooking.Api.Application.Abstractions;

namespace MeetingRoomBooking.Api.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
