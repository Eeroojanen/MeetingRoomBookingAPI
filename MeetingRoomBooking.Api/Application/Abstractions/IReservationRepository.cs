using MeetingRoomBooking.Api.Domain.Entities;

namespace MeetingRoomBooking.Api.Application.Abstractions;

public interface IReservationRepository
{
    IReadOnlyList<Reservation> GetByRoom(Guid roomId);

    (bool added, Reservation? conflicting) TryAdd(Reservation reservation);

    bool Remove(Guid roomId, Guid reservationId);
}
