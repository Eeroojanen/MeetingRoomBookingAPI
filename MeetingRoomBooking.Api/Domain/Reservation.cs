namespace MeetingRoomBooking.Api.Domain;

public sealed class Reservation
{
    public Reservation(
        Guid id,
        Guid roomId,
        string title,
        string organizer,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        RoomId = roomId;
        Title = title;
        Organizer = organizer;
        StartUtc = startUtc;
        EndUtc = endUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }
    public Guid RoomId { get; }
    public string Title { get; }
    public string Organizer { get; }
    public DateTimeOffset StartUtc { get; }
    public DateTimeOffset EndUtc { get; }
    public DateTimeOffset CreatedAtUtc { get; }

    public bool Overlaps(DateTimeOffset startUtc, DateTimeOffset endUtc)
        => startUtc < EndUtc && endUtc > StartUtc;
}
