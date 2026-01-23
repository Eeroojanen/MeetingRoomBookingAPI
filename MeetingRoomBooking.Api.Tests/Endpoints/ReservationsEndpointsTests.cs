using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeetingRoomBooking.Api.Contracts.Requests;
using MeetingRoomBooking.Api.Contracts.Responses;
using MeetingRoomBooking.Api.Domain.Entities;
using MeetingRoomBooking.Api.Tests.TestHost;
using MeetingRoomBooking.Api.Tests.TestHost;
using Xunit;
using Moq;

namespace MeetingRoomBooking.Api.Tests.Endpoints;

public sealed class ReservationsEndpointsTests
{
    private static readonly Guid RoomId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task GetRooms_Returns200AndList()
    {
        var factory = new ApiFactory();

        // The /api/rooms endpoint uses IRoomCatalog.GetAllRooms()
        factory.RoomCatalogMock
            .Setup(x => x.GetAllRooms())
            .Returns(new List<Room>
            {
                new(RoomId, "Neon", "Floor 1", 6),
            });

        var client = factory.CreateClient();

        var res = await client.GetAsync("/api/rooms");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadFromJsonAsync<List<RoomResponse>>();
        body.Should().NotBeNull();
        body!.Should().HaveCount(1);
        body[0].Id.Should().Be(RoomId);
    }

    [Fact]
    public async Task ListReservations_RoomNotFound_Returns404()
    {
        var factory = new ApiFactory();

        factory.RoomCatalogMock
            .Setup(x => x.RoomExists(RoomId))
            .Returns(false);

        var client = factory.CreateClient();

        var res = await client.GetAsync($"/api/rooms/{RoomId}/reservations");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListReservations_Returns200AndSorted()
    {
        var factory = new ApiFactory();

        factory.RoomCatalogMock.Setup(x => x.RoomExists(RoomId)).Returns(true);

        var r1 = new Reservation(Guid.NewGuid(), RoomId, "B", "Eero",
            DateTimeOffset.Parse("2026-01-21T11:00:00Z"),
            DateTimeOffset.Parse("2026-01-21T12:00:00Z"),
            DateTimeOffset.Parse("2026-01-20T09:00:00Z"));

        var r2 = new Reservation(Guid.NewGuid(), RoomId, "A", "Eero",
            DateTimeOffset.Parse("2026-01-21T10:00:00Z"),
            DateTimeOffset.Parse("2026-01-21T11:00:00Z"),
            DateTimeOffset.Parse("2026-01-20T09:00:00Z"));

        factory.ReservationRepositoryMock
            .Setup(x => x.GetByRoom(RoomId))
            .Returns(new List<Reservation> { r1, r2 });

        var client = factory.CreateClient();

        var res = await client.GetAsync($"/api/rooms/{RoomId}/reservations");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadFromJsonAsync<List<ReservationResponse>>();
        body.Should().NotBeNull();
        body!.Should().HaveCount(2);
        body[0].StartUtc.Should().Be(r2.StartUtc); // sorted ascending by StartUtc
        body[1].StartUtc.Should().Be(r1.StartUtc);
    }

    [Fact]
    public async Task CreateReservation_RoomNotFound_Returns404()
    {
        var factory = new ApiFactory();

        factory.RoomCatalogMock.Setup(x => x.RoomExists(RoomId)).Returns(false);

        var client = factory.CreateClient();

        var req = new CreateReservationRequest(
            Title: "Team sync",
            Organizer: "Eero",
            StartUtc: DateTimeOffset.Parse("2026-01-21T10:00:00Z"),
            EndUtc: DateTimeOffset.Parse("2026-01-21T11:00:00Z"));

        var res = await client.PostAsJsonAsync($"/api/rooms/{RoomId}/reservations", req);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateReservation_InvalidTimeRange_Returns400()
    {
        var factory = new ApiFactory();

        factory.RoomCatalogMock.Setup(x => x.RoomExists(RoomId)).Returns(true);
        factory.ClockMock.Setup(x => x.UtcNow).Returns(DateTimeOffset.Parse("2026-01-20T09:00:00Z"));

        var client = factory.CreateClient();

        var req = new CreateReservationRequest(
            Title: "Bad range",
            Organizer: "Eero",
            StartUtc: DateTimeOffset.Parse("2026-01-21T11:00:00Z"),
            EndUtc: DateTimeOffset.Parse("2026-01-21T10:00:00Z"));

        var res = await client.PostAsJsonAsync($"/api/rooms/{RoomId}/reservations", req);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReservation_InThePast_Returns400()
    {
        var factory = new ApiFactory();

        factory.RoomCatalogMock.Setup(x => x.RoomExists(RoomId)).Returns(true);
        factory.ClockMock.Setup(x => x.UtcNow).Returns(DateTimeOffset.Parse("2026-01-21T10:00:00Z"));

        var client = factory.CreateClient();

        var req = new CreateReservationRequest(
            Title: "Past meeting",
            Organizer: "Eero",
            StartUtc: DateTimeOffset.Parse("2026-01-21T09:00:00Z"),
            EndUtc: DateTimeOffset.Parse("2026-01-21T10:00:00Z"));

        var res = await client.PostAsJsonAsync($"/api/rooms/{RoomId}/reservations", req);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReservation_Overlapping_Returns409()
    {
        var factory = new ApiFactory();

        factory.RoomCatalogMock.Setup(x => x.RoomExists(RoomId)).Returns(true);
        factory.ClockMock.Setup(x => x.UtcNow).Returns(DateTimeOffset.Parse("2026-01-20T09:00:00Z"));

        // Make repo say "conflict"
        var conflicting = new Reservation(
            Guid.NewGuid(),
            RoomId,
            "Existing",
            "Someone",
            DateTimeOffset.Parse("2026-01-21T10:00:00Z"),
            DateTimeOffset.Parse("2026-01-21T11:00:00Z"),
            DateTimeOffset.Parse("2026-01-20T09:00:00Z"));

        factory.ReservationRepositoryMock
            .Setup(x => x.TryAdd(It.IsAny<Reservation>()))
            .Returns((false, conflicting));

        var client = factory.CreateClient();

        var req = new CreateReservationRequest(
            Title: "New one",
            Organizer: "Eero",
            StartUtc: DateTimeOffset.Parse("2026-01-21T10:30:00Z"),
            EndUtc: DateTimeOffset.Parse("2026-01-21T11:30:00Z"));

        var res = await client.PostAsJsonAsync($"/api/rooms/{RoomId}/reservations", req);
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateReservation_Success_Returns201AndBody()
    {
        var factory = new ApiFactory();

        factory.RoomCatalogMock.Setup(x => x.RoomExists(RoomId)).Returns(true);
        factory.ClockMock.Setup(x => x.UtcNow).Returns(DateTimeOffset.Parse("2026-01-20T09:00:00Z"));

        // Make repo accept reservation
        factory.ReservationRepositoryMock
            .Setup(x => x.TryAdd(It.IsAny<Reservation>()))
            .Returns((true, (Reservation?)null));

        var client = factory.CreateClient();

        var req = new CreateReservationRequest(
            Title: "Team sync",
            Organizer: "Eero",
            StartUtc: DateTimeOffset.Parse("2026-01-21T10:00:00Z"),
            EndUtc: DateTimeOffset.Parse("2026-01-21T11:00:00Z"));

        var res = await client.PostAsJsonAsync($"/api/rooms/{RoomId}/reservations", req);
        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await res.Content.ReadFromJsonAsync<ReservationResponse>();
        body.Should().NotBeNull();
        body!.RoomId.Should().Be(RoomId);
        body.Title.Should().Be("Team sync");
        body.Organizer.Should().Be("Eero");

        // Ensure repository was called
        factory.ReservationRepositoryMock.Verify(x => x.TryAdd(It.IsAny<Reservation>()), Times.Once);
    }

    [Fact]
    public async Task CancelReservation_Success_Returns204()
    {
        var factory = new ApiFactory();

        var reservationId = Guid.NewGuid();
        factory.RoomCatalogMock.Setup(x => x.RoomExists(RoomId)).Returns(true);

        factory.ReservationRepositoryMock
            .Setup(x => x.Remove(RoomId, reservationId))
            .Returns(true);

        var client = factory.CreateClient();

        var res = await client.DeleteAsync($"/api/rooms/{RoomId}/reservations/{reservationId}");
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CancelReservation_NotFound_Returns404()
    {
        var factory = new ApiFactory();

        var reservationId = Guid.NewGuid();
        factory.RoomCatalogMock.Setup(x => x.RoomExists(RoomId)).Returns(true);

        factory.ReservationRepositoryMock
            .Setup(x => x.Remove(RoomId, reservationId))
            .Returns(false);

        var client = factory.CreateClient();

        var res = await client.DeleteAsync($"/api/rooms/{RoomId}/reservations/{reservationId}");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
