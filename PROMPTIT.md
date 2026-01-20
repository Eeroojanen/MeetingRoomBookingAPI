Teköäly prompti: "I want to you make a simple meeting room booking API. Design a simple RESTful API for managing meeting room reservations. Core features should be creating a reservation nook a specific meeting room for a given time interval, cancel a reservation remove an existing booking and view reservations list all reservations for a specific meeting room. Rules for API No overlapping booking, No past reservations and Valid time range. Use C# and .NET for this project"

Tekoäly vastaus: "Below is a complete, simple ASP.NET Core Web API project for a Meeting Room Booking System with the rules you listed:

No overlapping bookings (for the same room)

No past reservations (start time must be >= “now”, using UTC)

Valid time range (end must be > start)

It uses an in-memory repository (so you can run it immediately without a DB). You can later swap the repository for EF Core/Cosmos/etc.

Project name

Recommended: MeetingRoomBooking.Api (cleaner for a solution that may later include .Domain, .Infrastructure, etc.)

API design
Endpoints

GET /api/rooms
List meeting rooms (seeded).

GET /api/rooms/{roomId}/reservations
List reservations for a room. Optional query:

?fromUtc=...&toUtc=... (ISO-8601 UTC)

POST /api/rooms/{roomId}/reservations
Create a reservation in a room for a time interval (UTC).

DELETE /api/rooms/{roomId}/reservations/{reservationId}
Cancel (remove) a reservation.

Reservation overlap rule

Two reservations overlap if:

newStart < existingEnd && newEnd > existingStart"

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

Tekoäly prompti: "error CS0266: Cannot implicitly convert type 'System.Collections.Generic.IEnumerable<MeetingRoomBooking.Api.Domain.Reservation>' to 'System.Collections.Generic.IReadOnlyList<MeetingRoomBooking.Api.Domain.Reservation>'. An explicit conversion exists (are you missing a cast?)" 

Tekäly vastaus: "Good catch — this is a classic C# type issue.
The error happens because after filtering with Where(...), the variable becomes an IEnumerable<Reservation>, but the method signature (and later code) expects an IReadOnlyList<Reservation>.

Specifically in Program.cs inside:

var reservations = repo.GetByRoom(roomId);

if (fromUtc is not null)
    reservations = reservations.Where(r => r.StartUtc >= fromUtc.Value);

if (toUtc is not null)
    reservations = reservations.Where(r => r.EndUtc <= toUtc.Value);


repo.GetByRoom(...) returns IReadOnlyList<Reservation>,
but .Where(...) returns IEnumerable<Reservation> → incompatible assignment.

✅ Fix (recommended)

Change the variable type to IEnumerable<Reservation> and materialize to a list only at the end.

Replace that whole block with this:
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


This way:

Internally we use IEnumerable for filtering

At the end we convert to a concrete List<T> for the response

🔍 Why this happens

IReadOnlyList<T> is more specific than IEnumerable<T>

LINQ operators (Where, Select, etc.) always return IEnumerable<T>

C# does not allow assigning a more general type back into a more specific one"

----------------------------------------------------------------------------------------------

Tekoäly prompt: "Generate me a http.rest file which allows me to test api endpoints"

Tekoäly vastaus: "Here’s a ready-to-use http.rest file for VS Code’s REST Client extension (Huachao Mao). It covers:

list rooms

list reservations for a room

create a reservation (future)

create overlap (expects 409)

create past (expects 400)

create invalid time range (expects 400)

cancel reservation (expects 204 once you paste a real reservation id)

Update @baseUrl if your port changes (yours is 5177).

@baseUrl = http://localhost:5177

# Seeded room IDs from InMemoryRoomCatalog (change if you changed seeds)
@roomId = 11111111-1111-1111-1111-111111111111

# Paste a reservation id here after creating one (GET list shows it)
@reservationId = 00000000-0000-0000-0000-000000000000


### Health check (just to confirm server responds)
GET {{baseUrl}}/api/rooms


### List rooms (same as above, but kept separate)
GET {{baseUrl}}/api/rooms


### List reservations for a room
GET {{baseUrl}}/api/rooms/{{roomId}}/reservations


### List reservations for a room with optional filters (use ISO-8601 UTC)
# Example:
# GET {{baseUrl}}/api/rooms/{{roomId}}/reservations?fromUtc=2026-01-21T00:00:00Z&toUtc=2026-01-22T00:00:00Z
GET {{baseUrl}}/api/rooms/{{roomId}}/reservations?fromUtc=2026-01-21T00:00:00Z&toUtc=2026-01-22T00:00:00Z


### Create a reservation (MUST be in the future, UTC)
# Expected: 201 Created
POST {{baseUrl}}/api/rooms/{{roomId}}/reservations
Content-Type: application/json

{
  "title": "Team sync",
  "organizer": "Eero",
  "startUtc": "2026-01-21T10:00:00Z",
  "endUtc": "2026-01-21T11:00:00Z"
}


### Create an overlapping reservation (should fail)
# Expected: 409 Conflict
POST {{baseUrl}}/api/rooms/{{roomId}}/reservations
Content-Type: application/json

{
  "title": "Overlapping meeting",
  "organizer": "Eero",
  "startUtc": "2026-01-21T10:30:00Z",
  "endUtc": "2026-01-21T11:30:00Z"
}


### Create a reservation in the past (should fail)
# Expected: 400 Bad Request
POST {{baseUrl}}/api/rooms/{{roomId}}/reservations
Content-Type: application/json

{
  "title": "Past meeting",
  "organizer": "Eero",
  "startUtc": "2020-01-01T10:00:00Z",
  "endUtc": "2020-01-01T11:00:00Z"
}


### Create a reservation with invalid time range (end <= start)
# Expected: 400 Bad Request
POST {{baseUrl}}/api/rooms/{{roomId}}/reservations
Content-Type: application/json

{
  "title": "Invalid range",
  "organizer": "Eero",
  "startUtc": "2026-01-21T12:00:00Z",
  "endUtc": "2026-01-21T12:00:00Z"
}


### Cancel a reservation (paste a real id into @reservationId first)
# Expected: 204 No Content
DELETE {{baseUrl}}/api/rooms/{{roomId}}/reservations/{{reservationId}}


### Cancel a reservation that doesn't exist (should fail)
# Expected: 404 Not Found
DELETE {{baseUrl}}/api/rooms/{{roomId}}/reservations/99999999-9999-9999-9999-999999999999


### List reservations again (verify cancel worked)
GET {{baseUrl}}/api/rooms/{{roomId}}/reservations


Tip for workflow:

Run Create a reservation → copy the "id" from the response or from the reservation list.

Paste it into @reservationId.

Run the DELETE request.

If you want, I can tweak this so the times are always “future” relative to today (REST Client can’t compute dynamic times easily, but we can make it easier with a few preset date blocks).


------------------------------------------------------------------------------------------------------------------

Tekoäly prompt: "Create xUnit test cases for my ASP.NET Core Web API endpoints and use Moq to mock all data dependencies."

Tekoäly vastaus: " Below is a complete set of xUnit endpoint tests for your Minimal API using WebApplicationFactory + Moq to mock all data dependencies (IRoomCatalog, IReservationRepository, IClock, and even IReservationService where useful).

I’m assuming your API project is named MeetingRoomBooking.Api and contains the interfaces exactly like we built.

0) One small change required in your API project

Minimal APIs need a Program class exposed for WebApplicationFactory.

Add this at the bottom of Program.cs (outside any namespace):

public partial class Program { }


That’s it.

1) Create a test project

From the solution folder:

dotnet new xunit -n MeetingRoomBooking.Api.Tests
dotnet add MeetingRoomBooking.Api.Tests reference MeetingRoomBooking.Api/MeetingRoomBooking.Api.csproj


Add packages:

dotnet add MeetingRoomBooking.Api.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet add MeetingRoomBooking.Api.Tests package Moq
dotnet add MeetingRoomBooking.Api.Tests package xunit.runner.visualstudio


(Optional but recommended)

dotnet add MeetingRoomBooking.Api.Tests package FluentAssertions

2) MeetingRoomBooking.Api.Tests.csproj

Make sure your test .csproj looks roughly like this (net8.0 is fine even if the app is net8.0):

<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="xunit" Version="2.6.6" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MeetingRoomBooking.Api\MeetingRoomBooking.Api.csproj" />
  </ItemGroup>

</Project>


(Exact versions aren’t critical as long as they restore.)

3) Test factory that injects mocks

Create: TestHost/ApiFactory.cs

using MeetingRoomBooking.Api.Services;
using MeetingRoomBooking.Api.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MeetingRoomBooking.Api.Tests.TestHost;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public Mock<IRoomCatalog> RoomCatalogMock { get; } = new();
    public Mock<IReservationRepository> ReservationRepositoryMock { get; } = new();
    public Mock<IClock> ClockMock { get; } = new();
    public Mock<IReservationService> ReservationServiceMock { get; } = new();

    // By default, we test real endpoint -> real ReservationService
    // so we won't register ReservationServiceMock unless a test wants it.
    public bool UseMockReservationService { get; set; } = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing registrations from the API project
            RemoveAll<IRoomCatalog>(services);
            RemoveAll<IReservationRepository>(services);
            RemoveAll<IClock>(services);
            RemoveAll<IReservationService>(services);

            // Register mocks
            services.AddSingleton(RoomCatalogMock.Object);
            services.AddSingleton(ReservationRepositoryMock.Object);
            services.AddSingleton(ClockMock.Object);

            if (UseMockReservationService)
            {
                services.AddSingleton(ReservationServiceMock.Object);
            }
            else
            {
                // Use real business rules with mocked data deps
                services.AddSingleton<IReservationService, ReservationService>();
            }
        });
    }

    private static void RemoveAll<T>(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var d in descriptors)
            services.Remove(d);
    }
}

4) Endpoint tests (real service, mocked data deps)

Create: Endpoints/ReservationsEndpointsTests.cs

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeetingRoomBooking.Api.Contracts;
using MeetingRoomBooking.Api.Domain;
using MeetingRoomBooking.Api.Tests.TestHost;
using Moq;
using Xunit;

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


These tests:

spin up your API in-memory

hit real HTTP endpoints with HttpClient

mock all data dependencies used by the endpoints/service

assert correct HTTP status codes + response bodies

5) Run tests

From solution folder:

dotnet test

Notes & common gotchas
If you get “entry point not found” / WebApplicationFactory issues

You likely forgot:

public partial class Program { }

If swagger runs but tests fail to start

Make sure test project references the API project and uses the right WebApplicationFactory<Program>."

---------------------------------------------------------------------------------------------------------------------------


