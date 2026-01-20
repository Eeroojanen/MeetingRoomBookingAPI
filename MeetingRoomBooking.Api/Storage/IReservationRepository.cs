using MeetingRoomBooking.Api.Domain;

namespace MeetingRoomBooking.Api.Storage;

public interface IReservationRepository
{
    IReadOnlyList<Reservation> GetByRoom(Guid roomId);

    /// <summary>
    /// Attempts to add a reservation if it does not overlap an existing reservation.
    /// Returns (true, null) on success; (false, conflictingReservation) on conflict.
    /// </summary>
    (bool added, Reservation? conflicting) TryAdd(Reservation reservation);

    bool Remove(Guid roomId, Guid reservationId);
}
