using MeetingRoomBooking.Api.Contracts;
using MeetingRoomBooking.Api.Domain;
using MeetingRoomBooking.Api.Storage;

namespace MeetingRoomBooking.Api.Services;

public sealed class ReservationService : IReservationService
{
    private static readonly TimeSpan MaxReservationLength = TimeSpan.FromHours(12);

    private readonly IRoomCatalog _rooms;
    private readonly IReservationRepository _repo;
    private readonly IClock _clock;

    public ReservationService(IRoomCatalog rooms, IReservationRepository repo, IClock clock)
    {
        _rooms = rooms;
        _repo = repo;
        _clock = clock;
    }

    public Task<Result<Reservation>> CreateAsync(Guid roomId, CreateReservationRequest request)
    {
        // Room exists?
        if (!_rooms.RoomExists(roomId))
        {
            return Task.FromResult(Result<Reservation>.Fail(new ApiError(
                "Room not found",
                $"No meeting room exists with id '{roomId}'.",
                StatusCodes.Status404NotFound)));
        }

        // Basic field validation
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Task.FromResult(Result<Reservation>.Fail(new ApiError(
                "Invalid request",
                "Title is required.",
                StatusCodes.Status400BadRequest)));
        }

        if (string.IsNullOrWhiteSpace(request.Organizer))
        {
            return Task.FromResult(Result<Reservation>.Fail(new ApiError(
                "Invalid request",
                "Organizer is required.",
                StatusCodes.Status400BadRequest)));
        }

        // Valid time range
        if (request.EndUtc <= request.StartUtc)
        {
            return Task.FromResult(Result<Reservation>.Fail(new ApiError(
                "Invalid time range",
                "EndUtc must be greater than StartUtc.",
                StatusCodes.Status400BadRequest)));
        }

        var length = request.EndUtc - request.StartUtc;
        if (length > MaxReservationLength)
        {
            return Task.FromResult(Result<Reservation>.Fail(new ApiError(
                "Invalid time range",
                $"Reservation length cannot exceed {MaxReservationLength.TotalHours:0} hours.",
                StatusCodes.Status400BadRequest)));
        }

        // No past reservations
        var now = _clock.UtcNow;
        if (request.StartUtc < now)
        {
            return Task.FromResult(Result<Reservation>.Fail(new ApiError(
                "Reservation in the past",
                $"StartUtc must be in the future (>= {now:O}). Use UTC timestamps.",
                StatusCodes.Status400BadRequest)));
        }

        var reservation = new Reservation(
            id: Guid.NewGuid(),
            roomId: roomId,
            title: request.Title.Trim(),
            organizer: request.Organizer.Trim(),
            startUtc: request.StartUtc,
            endUtc: request.EndUtc,
            createdAtUtc: now);

        // No overlapping booking
        var (added, conflicting) = _repo.TryAdd(reservation);
        if (!added)
        {
            return Task.FromResult(Result<Reservation>.Fail(new ApiError(
                "Overlapping reservation",
                $"Conflicts with an existing reservation ({conflicting!.StartUtc:O} - {conflicting.EndUtc:O}).",
                StatusCodes.Status409Conflict)));
        }

        return Task.FromResult(Result<Reservation>.Success(reservation));
    }

    public Result<Unit> Cancel(Guid roomId, Guid reservationId)
    {
        if (!_rooms.RoomExists(roomId))
        {
            return Result<Unit>.Fail(new ApiError(
                "Room not found",
                $"No meeting room exists with id '{roomId}'.",
                StatusCodes.Status404NotFound));
        }

        var removed = _repo.Remove(roomId, reservationId);
        if (!removed)
        {
            return Result<Unit>.Fail(new ApiError(
                "Reservation not found",
                $"No reservation '{reservationId}' exists in room '{roomId}'.",
                StatusCodes.Status404NotFound));
        }

        return Result<Unit>.Success(new Unit());
    }
}
