using System.Collections.Concurrent;
using MeetingRoomBooking.Api.Domain;

namespace MeetingRoomBooking.Api.Storage;

public sealed class InMemoryReservationRepository : IReservationRepository
{
    // roomId -> list of reservations for that room
    private readonly ConcurrentDictionary<Guid, List<Reservation>> _byRoom = new();

    // roomId -> lock object (keeps overlaps check + insert atomic)
    private readonly ConcurrentDictionary<Guid, object> _roomLocks = new();

    public IReadOnlyList<Reservation> GetByRoom(Guid roomId)
    {
        if (!_byRoom.TryGetValue(roomId, out var list))
            return Array.Empty<Reservation>();

        // return snapshot
        lock (GetLock(roomId))
        {
            return list.ToList();
        }
    }

    public (bool added, Reservation? conflicting) TryAdd(Reservation reservation)
    {
        var roomId = reservation.RoomId;
        var roomLock = GetLock(roomId);

        lock (roomLock)
        {
            var list = _byRoom.GetOrAdd(roomId, _ => new List<Reservation>());

            var conflict = list.FirstOrDefault(r => r.Overlaps(reservation.StartUtc, reservation.EndUtc));
            if (conflict is not null)
                return (false, conflict);

            list.Add(reservation);
            return (true, null);
        }
    }

    public bool Remove(Guid roomId, Guid reservationId)
    {
        var roomLock = GetLock(roomId);

        lock (roomLock)
        {
            if (!_byRoom.TryGetValue(roomId, out var list))
                return false;

            var index = list.FindIndex(r => r.Id == reservationId);
            if (index < 0)
                return false;

            list.RemoveAt(index);
            return true;
        }
    }

    private object GetLock(Guid roomId)
        => _roomLocks.GetOrAdd(roomId, _ => new object());
}
