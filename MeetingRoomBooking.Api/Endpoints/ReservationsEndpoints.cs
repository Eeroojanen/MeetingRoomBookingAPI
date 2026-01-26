using MeetingRoomBooking.Api.Application.Abstractions;
using MeetingRoomBooking.Api.Application.Reservations;
using MeetingRoomBooking.Api.Contracts.Requests;
using MeetingRoomBooking.Api.Contracts.Responses;
using MeetingRoomBooking.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MeetingRoomBooking.Api.Endpoints;

public static class ReservationsEndpoints
{
    public static IEndpointRouteBuilder MapReservationsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/rooms/{roomId:guid}/reservations",
            ([FromRoute] Guid roomId,
            [FromQuery] DateTimeOffset? fromUtc,
            [FromQuery] DateTimeOffset? toUtc,
            IRoomCatalog roomCatalog,
            IReservationRepository repo) =>
            {
                if (!roomCatalog.RoomExists(roomId))
                {
                    return Results.Problem(
                        title: "Room not found",
                        detail: $"No meeting room exists with id '{roomId}'.",
                        statusCode: StatusCodes.Status404NotFound);
                }

                if (fromUtc is not null && toUtc is not null && fromUtc > toUtc)
                {
                    return Results.Problem(
                        title: "Invalid date range",
                        detail: "fromUtc must be earlier than or equal to toUtc.",
                        statusCode: StatusCodes.Status400BadRequest);
                }

                IEnumerable<Reservation> reservations = repo.GetByRoom(roomId);

                if (fromUtc is not null)
                    reservations = reservations.Where(r => r.StartUtc >= fromUtc.Value);

                if (toUtc is not null)
                    reservations = reservations.Where(r => r.EndUtc <= toUtc.Value);

                var result = reservations
                    .OrderBy(r => r.StartUtc)
                    .Select(r => new ReservationResponse(
                        r.Id, r.RoomId, r.Title, r.Organizer, r.StartUtc, r.EndUtc, r.CreatedAtUtc))
                    .ToList();

                return Results.Ok(result);
            })
            .WithName("ListReservationsForRoom")
            .Produces<List<ReservationResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/rooms/{roomId:guid}/reservations",
            ([FromRoute] Guid roomId,
             CreateReservationRequest request,
             IReservationService service) =>
            {
                var outcome = service.Create(roomId, request);

                return outcome.Match<IResult>(
                    created => Results.Created(
                        $"/api/rooms/{roomId}/reservations/{created.Id}",
                        new ReservationResponse(
                            created.Id, created.RoomId, created.Title, created.Organizer,
                            created.StartUtc, created.EndUtc, created.CreatedAtUtc)),
                    error => Results.Problem(
                        title: error.Title,
                        detail: error.Detail,
                        statusCode: error.StatusCode));
            })
            .WithName("CreateReservation")
            .Produces<ReservationResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapDelete("/api/rooms/{roomId:guid}/reservations/{reservationId:guid}",
            ([FromRoute] Guid roomId,
             [FromRoute] Guid reservationId,
             IReservationService service) =>
            {
                var outcome = service.Cancel(roomId, reservationId);

                return outcome.Match<IResult>(
                    _ => Results.NoContent(),
                    error => Results.Problem(
                        title: error.Title,
                        detail: error.Detail,
                        statusCode: error.StatusCode));
            })
            .WithName("CancelReservation")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
