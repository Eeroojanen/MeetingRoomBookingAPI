using MeetingRoomBooking.Api.Domain.Entities;

namespace MeetingRoomBooking.Api.Application.Abstractions;

public interface IRoomCatalog
{
    IReadOnlyList<Room> GetAllRooms();
    bool RoomExists(Guid roomId);
}
