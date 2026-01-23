using MeetingRoomBooking.Api.Application.Abstractions;
using MeetingRoomBooking.Api.Application.Reservations;
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

    public bool UseMockReservationService { get; set; } = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            RemoveAll<IRoomCatalog>(services);
            RemoveAll<IReservationRepository>(services);
            RemoveAll<IClock>(services);
            RemoveAll<IReservationService>(services);

            services.AddSingleton(RoomCatalogMock.Object);
            services.AddSingleton(ReservationRepositoryMock.Object);
            services.AddSingleton(ClockMock.Object);

            if (UseMockReservationService)
                services.AddSingleton(ReservationServiceMock.Object);
            else
                services.AddSingleton<IReservationService, ReservationService>();
        });
    }

    private static void RemoveAll<T>(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var d in descriptors)
            services.Remove(d);
    }
}
