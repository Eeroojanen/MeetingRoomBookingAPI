using MeetingRoomBooking.Api.Services;
using MeetingRoomBooking.Api.Storage;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IRoomCatalog, InMemoryRoomCatalog>();
builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddSingleton<IReservationService, ReservationService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Helpful: ensure we always speak UTC in this API
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Timezone"] = "UTC";
    await next();
});

app.MapGet("/api/rooms", (IRoomCatalog rooms) =>
{
    var list = rooms.GetAllRooms()
        .Select(r => new MeetingRoomBooking.Api.Contracts.RoomResponse(
            r.Id, r.Name, r.Location, r.Capacity))
        .ToList();

    return Results.Ok(list);
})
.WithName("ListRooms")
.Produces<List<MeetingRoomBooking.Api.Contracts.RoomResponse>>(StatusCodes.Status200OK);

app.MapGet("/api/rooms/{roomId:guid}/reservations",
    ([FromRoute] Guid roomId,
     [FromQuery] DateTimeOffset? fromUtc,
     [FromQuery] DateTimeOffset? toUtc,
     IRoomCatalog roomCatalog,
     IReservationRepository repo) =>
    {
        if (!roomCatalog.RoomExists(roomId))
        {
            return Results.NotFound(Problem(
                title: "Room not found",
                detail: $"No meeting room exists with id '{roomId}'.",
                statusCode: StatusCodes.Status404NotFound));
        }

        IEnumerable<MeetingRoomBooking.Api.Domain.Reservation> reservations =
            repo.GetByRoom(roomId);

        if (fromUtc is not null)
            reservations = reservations.Where(r => r.StartUtc >= fromUtc.Value);

        if (toUtc is not null)
            reservations = reservations.Where(r => r.EndUtc <= toUtc.Value);

        var result = reservations
            .OrderBy(r => r.StartUtc)
            .Select(r => new MeetingRoomBooking.Api.Contracts.ReservationResponse(
                r.Id, r.RoomId, r.Title, r.Organizer, r.StartUtc, r.EndUtc, r.CreatedAtUtc))
            .ToList();

        return Results.Ok(result);
    })
    
.WithName("ListReservationsForRoom")
.Produces<List<MeetingRoomBooking.Api.Contracts.ReservationResponse>>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status404NotFound);

app.MapPost("/api/rooms/{roomId:guid}/reservations",
    async ([FromRoute] Guid roomId,
     MeetingRoomBooking.Api.Contracts.CreateReservationRequest request,
     IReservationService service) =>
    {
        var outcome = await service.CreateAsync(roomId, request);

        return outcome.Match<IResult>(
            created =>
            {
                // Location header points to the reservation list endpoint; you can also add a "get by id" endpoint later.
                return Results.Created($"/api/rooms/{roomId}/reservations",
                    new MeetingRoomBooking.Api.Contracts.ReservationResponse(
                        created.Id, created.RoomId, created.Title, created.Organizer,
                        created.StartUtc, created.EndUtc, created.CreatedAtUtc));
            },
            error => Results.Problem(
                title: error.Title,
                detail: error.Detail,
                statusCode: error.StatusCode));
    })
.WithName("CreateReservation")
.Produces<MeetingRoomBooking.Api.Contracts.ReservationResponse>(StatusCodes.Status201Created)
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

app.Run();

static ProblemDetails Problem(string title, string detail, int statusCode) =>
    new()
    {
        Title = title,
        Detail = detail,
        Status = statusCode
    };

public partial class Program { }

