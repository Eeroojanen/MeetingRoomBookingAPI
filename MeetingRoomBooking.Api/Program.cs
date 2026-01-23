using MeetingRoomBooking.Api.Application.Abstractions;
using MeetingRoomBooking.Api.Application.Reservations;
using MeetingRoomBooking.Api.Endpoints;
using MeetingRoomBooking.Api.Infrastructure.Storage;
using MeetingRoomBooking.Api.Infrastructure.Time;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI registrations (abstractions -> implementations)
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IRoomCatalog, InMemoryRoomCatalog>();
builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddSingleton<IReservationService, ReservationService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Register endpoints
app.MapRoomsEndpoints();
app.MapReservationsEndpoints();

app.Run();

public partial class Program { }
