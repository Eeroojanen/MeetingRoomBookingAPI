namespace MeetingRoomBooking.Api.Domain.Entities;

public sealed class Room
{
    public Room(Guid id, string name, string location, int capacity)
    {
        Id = id;
        Name = name;
        Location = location;
        Capacity = capacity;
    }

    public Guid Id { get; }
    public string Name { get; }
    public string Location { get; }
    public int Capacity { get; }
}
