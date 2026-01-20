using MeetingRoomBooking.Api.Domain;

namespace MeetingRoomBooking.Api.Storage;

public interface IRoomCatalog
{
    IReadOnlyList<Room> GetAllRooms();
    bool RoomExists(Guid roomId);
}
