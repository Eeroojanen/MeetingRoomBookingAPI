# Meeting Room Booking API

Minimal ASP.NET Core API for listing meeting rooms and managing reservations. The service uses in-memory storage and exposes REST endpoints for rooms and reservations.

## Features
- List available rooms.
- List reservations for a room with optional time filters.
- Create a reservation with validation (time range, overlaps, UTC timestamps).
- Cancel reservations.
- Swagger/OpenAPI UI enabled.

## Requirements
- .NET 8 SDK

## Run the API
```bash
dotnet run --project MeetingRoomBooking.Api
```

The API will start and expose Swagger UI at:
- `http://localhost:5177/swagger`

> Port may differ depending on launch settings.

## Endpoints

### Rooms
- `GET /api/rooms` — List rooms.

### Reservations
- `GET /api/rooms/{roomId}/reservations` — List reservations for a room.
  - Optional query params: `fromUtc`, `toUtc` (ISO-8601, UTC)
  - Returns **400** when `fromUtc > toUtc`.

- `POST /api/rooms/{roomId}/reservations` — Create a reservation.
  - Returns **201** and `Location: /api/rooms/{roomId}/reservations/{reservationId}`
  - Validation errors return **400**
  - Overlaps return **409**

- `DELETE /api/rooms/{roomId}/reservations/{reservationId}` — Cancel a reservation.
  - Returns **204** on success, **404** if not found.

## Validation Rules
Create reservation requests must satisfy:
- `title` and `organizer` are required
- `startUtc < endUtc`
- Reservation length ≤ 12 hours
- `startUtc` must be in the future
- `startUtc` and `endUtc` **must be UTC** (`Offset == +00:00`)
- No overlapping reservations in the same room

List reservations query:
- If both `fromUtc` and `toUtc` are provided, `fromUtc` must be ≤ `toUtc`

## Testing with http.rest
The repository includes a `http.rest` file at the root with ready-made requests, including:
- Successful reservation creation
- Overlap conflict
- Invalid ranges
- Non-UTC timestamp validation
- Cancellation

Open `http.rest` in VS Code and execute requests with the REST Client extension.

## In-Memory Storage Notes
- Rooms are stored in an immutable in-memory list.
- Reservations are stored in a concurrent dictionary with per-room locks.

## Project Structure
- `MeetingRoomBooking.Api/Endpoints` — Minimal API endpoints
- `MeetingRoomBooking.Api/Application` — Services, abstractions, and result types
- `MeetingRoomBooking.Api/Domain` — Domain entities
- `MeetingRoomBooking.Api/Infrastructure` — In-memory repositories and time provider

## License
Internal/educational use.