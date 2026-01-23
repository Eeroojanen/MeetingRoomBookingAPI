using MeetingRoomBooking.Api.Application.Common;
using MeetingRoomBooking.Api.Contracts.Requests;
using MeetingRoomBooking.Api.Domain.Entities;

namespace MeetingRoomBooking.Api.Application.Reservations;

public interface IReservationService
{
    Task<Result<Reservation>> CreateAsync(Guid roomId, CreateReservationRequest request);
    Result<Unit> Cancel(Guid roomId, Guid reservationId);
}
