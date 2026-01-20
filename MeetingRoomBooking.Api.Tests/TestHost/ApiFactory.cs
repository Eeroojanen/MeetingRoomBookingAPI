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
