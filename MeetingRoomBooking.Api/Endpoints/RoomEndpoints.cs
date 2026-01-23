using MeetingRoomBooking.Api.Application.Abstractions;
using MeetingRoomBooking.Api.Contracts.Responses;

namespace MeetingRoomBooking.Api.Endpoints;

public static class RoomsEndpoints
{
    public static IEndpointRouteBuilder MapRoomsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/rooms", (IRoomCatalog rooms) =>
        {
            var list = rooms.GetAllRooms()
                .Select(r => new RoomResponse(r.Id, r.Name, r.Location, r.Capacity))
                .ToList();

            return Results.Ok(list);
        })
        .WithName("ListRooms")
        .Produces<List<RoomResponse>>(StatusCodes.Status200OK);

        return app;
    }
}
