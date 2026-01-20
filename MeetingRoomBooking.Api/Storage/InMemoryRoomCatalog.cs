using MeetingRoomBooking.Api.Domain;

namespace MeetingRoomBooking.Api.Storage;

public sealed class InMemoryRoomCatalog : IRoomCatalog
{
    private readonly List<Room> _rooms = new()
    {
        new Room(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Neon",    "Floor 1", 6),
        new Room(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Aurora",  "Floor 2", 10),
        new Room(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Nimbus",  "Floor 3", 4),
    };

    public IReadOnlyList<Room> GetAllRooms() => _rooms;

    public bool RoomExists(Guid roomId) => _rooms.Any(r => r.Id == roomId);
}
