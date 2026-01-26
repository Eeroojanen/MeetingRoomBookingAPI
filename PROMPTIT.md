# PROMPTIT.md  
AI Prompt Documentation – Meeting Room Booking API Project

This file documents the prompts used with an AI assistant and the essential parts of the responses that guided the development of the Meeting Room Booking API project.  
Some AI responses have been slightly shortened to include only the most relevant information.

---

## Prompt 1

**Tekoäly prompti:**

"I want to you make a simple meeting room booking API. Design a simple RESTful API for managing meeting room reservations. Core features should be creating a reservation nook a specific meeting room for a given time interval, cancel a reservation remove an existing booking and view reservations list all reservations for a specific meeting room. Rules for API No overlapping booking, No past reservations and Valid time range. Use C# and .NET for this project"

**Tekoäly vastaus (tiivistetty):**

Provided a complete ASP.NET Core Web API design for a Meeting Room Booking System with:

- Endpoints for creating, listing and cancelling reservations  
- Business rules:
  - No overlapping reservations  
  - No reservations in the past  
  - Start time must be before end time  

- Suggested using an in-memory repository for storage  
- Defined the overlap rule:  
  `newStart < existingEnd && newEnd > existingStart`

---

## Prompt 2

**Tekoäly prompti:**

"error CS0266: Cannot implicitly convert type 'System.Collections.Generic.IEnumerable<MeetingRoomBooking.Api.Domain.Reservation>' to 'System.Collections.Generic.IReadOnlyList<MeetingRoomBooking.Api.Domain.Reservation>'. An explicit conversion exists (are you missing a cast?)"

**Tekoäly vastaus (tiivistetty):**

Explained that LINQ `Where()` returns `IEnumerable<T>`, which cannot be assigned to `IReadOnlyList<T>`.

Suggested fix:

- Use `IEnumerable<Reservation>` while filtering  
- Convert to a list at the end using `.ToList()`

This resolved the type conversion error.

---

## Prompt 3

**Tekoäly prompt:**

"Generate me a http.rest file which allows me to test api endpoints"

**Tekoäly vastaus (tiivistetty):**

Generated a `http.rest` file for VS Code REST Client containing:

- Requests for:
  - Listing rooms  
  - Listing reservations  
  - Creating reservations (valid, overlapping, past, invalid range)  
  - Cancelling reservations  

- Included example JSON bodies and expected HTTP status codes (201, 400, 409, 204).

---

## Prompt 4

**Tekoäly prompt:**

"Create xUnit test cases for my ASP.NET Core Web API endpoints and use Moq to mock all data dependencies."

**Tekoäly vastaus (tiivistetty):**

Provided an automated testing setup using:

- xUnit with `WebApplicationFactory`  
- Moq to mock all data dependencies (IRoomCatalog, IReservationRepository, IClock, IReservationService)  

Included:

- Test factory to inject mocked services  
- Example endpoint tests for:
  - Listing rooms and reservations  
  - Creating reservations (success, invalid, past, overlapping)  
  - Cancelling reservations (success and not found)  

Also instructed to add:

"```csharp
public partial class Program { }"

---

## Prompt 5

**Tekoäly prompt:**

"Separate API plumbing from business logic, group things by responsibility (Application vs Infrastructure), keep Domain clean (no DTOs, no storage concerns), reduce Program.cs bloat by moving endpoint mapping out, and prepare the project for swapping InMemory → EF Core later without touching business logic."

**Tekoäly vastaus (tiivistetty):**

Suggested reorganizing the project using a Clean Architecture approach:

- Keep Domain clean and independent  
- Move interfaces to Application layer  
- Move implementations to Infrastructure layer  
- Move business services to Application  
- Extract endpoint mappings into separate files  
- Keep `Program.cs` minimal  

This prepares the project for replacing the in-memory storage with EF Core later without changing business logic.

---

## Prompt 6

**Tekoäly prompt:**

"My CreateReservation endpoint returns a 201 Created response with a Location header that does not include the new resource ID (it returns /api/rooms/{roomId}/reservations). Shouldn’t the Location header point to /api/rooms/{roomId}/reservations/{reservationId} instead?"

**Tekoäly vastaus (tiivistetty):**

Confirmed that the Location header in a 201 Created response should point to the newly created resource, not the collection:

The correct URL format is
/api/rooms/{roomId}/reservations/{reservationId}

The current value does not uniquely identify the created reservation

The endpoint should include the reservation ID in the Location header

Recommended using a named route (CreatedAtRoute) instead of a hard-coded URL for better maintainability

This makes the API follow proper REST conventions and ensures clients receive a canonical URL for the created resource.

---

## Prompt 7

**Tekoäly prompt:**

"The GET listing endpoint currently does not validate the logical order of the fromUtc and toUtc query parameters. When fromUtc > toUtc, it simply returns an empty result. I want the API to return a 400 Bad Request with a clear validation error message in this case."

**Tekoäly vastaus (tiivistetty):**

Confirmed that the API should validate the logical order of the fromUtc and toUtc query parameters.

When fromUtc > toUtc, the request represents an invalid date range and should not be treated as a normal query.

The current behavior returns an empty list with a 200 OK response, which is misleading because it hides the fact that the client provided invalid input.

Recommended adding an explicit validation check before applying filters.

When both parameters are provided and fromUtc is later than toUtc, the endpoint should return a 400 Bad Request.

Suggested returning a clear validation error message such as:
"fromUtc must be earlier than or equal to toUtc."

This makes the API behavior more predictable, helps clients detect invalid requests, and clearly distinguishes between an empty result and a logically invalid query.

---

## Prompt 8

**Tekoäly prompt:**

"The CreateReservation endpoint currently accepts DateTimeOffset values that are not in UTC (e.g. StartUtc.Offset != TimeSpan.Zero).

Even though the request schema/documentation requires UTC timestamps, this is not validated.

I want the API to reject non-UTC values and return a 400 Bad Request with a clear validation error message."

**Tekoäly vastaus (tiivistetty):**

Add a validation in ReservationService.CreateAsync to reject non-UTC DateTimeOffset values.
The check is placed after the title/organizer validation and before the time-range validation.

If StartUtc.Offset or EndUtc.Offset ≠ TimeSpan.Zero
→ return 400 Bad Request
→ error: “StartUtc and EndUtc must be provided in UTC (offset +00:00).”

Goal: ensure all timestamps are always provided in UTC (+00:00).

---

## Prompt 9

**Tekoäly prompt:**

The Result<T>.Match method currently uses Value! to bypass nullability checks.

If Success == true but Value is null, this can cause a NullReferenceException when Match tries to access it.

I want to make this API safe against null values.

Should I add a generic constraint like where T : notnull, add a runtime check, or both?

How should I change the code to prevent this issue?

**Tekoäly vastaus (tiivistetty):**

Recommendation: add both compile-time and runtime null-safety to Result<T>:

Compile-time guard: add where T : notnull to Result<T>
→ prevents creating types like Result<string?>.

Runtime guard: add a null check in Match (or in the Success factory)
→ throws a clear exception if a “successful” result contains a null value, instead of causing a later NRE.

Why:

where T : notnull blocks null success values at compile time.

Runtime checks protect against nulls still leaking in (e.g., via reflection, serialization, or broken call sites).

Minimal option:
Only add the null check to the Success factory for lightweight protection.

---

## Prompt 10

**Tekoäly prompt:**

The CreateAsync method currently uses Task.FromResult(...) in several return paths.

The method does not perform any real asynchronous operations.

I want to simplify this by either making the method synchronous or converting it to an async method and returning results directly.

**Tekoäly vastaus (tiivistetty):**

Because CreateAsync performs no real asynchronous work, the cleanest solution is to make it synchronous.
This requires updating the interface and all call sites.

Option A (recommended): Make it synchronous

Change the interface and implementation:

Task<Result<Reservation>> CreateAsync(...)
→ Result<Reservation> Create(...)

Replace all:

Task.FromResult(Result<Reservation>.Fail(...)) / Success(...)
with:

return Result<Reservation>.Fail(...) / Success(...)

Update call sites (e.g. endpoints):

await service.CreateAsync(...)
→ service.Create(...) (no await)

Why:
The method is fully synchronous, so its signature should reflect that. This simplifies the code and avoids unnecessary Task allocations.

Option B: Keep the async signature (not recommended)
public async Task<Result<Reservation>> CreateAsync(...)


No await → compiler warning

Still creates a Task unnecessarily

Adds async overhead with no benefit

Recommendation

Option A is the clearest and most correct solution in the current state.
You can always switch back to CreateAsync later if real async I/O (e.g. DB calls) is added.

---

## Prompt 11

**Tekoäly prompt:**

The reservation repository uses locks, but the room catalog (InMemoryRoomCatalog) does not.

This means the overall in-memory implementation is not fully thread-safe.

If the application runs in a multi-threaded environment, callers could observe inconsistent state or modify shared data concurrently.

Would it be better to make the room list immutable (e.g. readonly, IReadOnlyList, or a defensive copy)?

What is the recommended approach to make this design thread-safe?

**Tekoäly vastaus (tiivistetty):**

Recommendation: Make the rooms list immutable and expose only a read-only view or a copy.
This gives you thread safety without locks.

Simple and safe approach

Keep _rooms readonly and never modify it after initialization.

Return a read-only view or a copy:

Use AsReadOnly() if the list never changes (no allocations).

Use ToList() if you want a fully detached copy on each call.

Why this is thread-safe

When data never changes, concurrent reads are safe.

The only real risk is external code mutating the internal list → prevented by returning read-only/copies.

Example (lightweight, thread-safe)
private readonly IReadOnlyList<Room> _rooms = new List<Room>
{
    new Room(...)
}.AsReadOnly();

public IReadOnlyList<Room> GetAllRooms() => _rooms;

When would locks be needed?

If rooms are added or removed at runtime:

Use a ConcurrentDictionary, or

Protect mutations with lock and return copies.

Summary:

If the list is static → immutability + read-only exposure is the cleanest and most efficient.

If the list can change → you need locks or a thread-safe collection.

---