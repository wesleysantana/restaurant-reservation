using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Application.Services;
using RestaurantReservation.Domain.Repositories;
using RestaurantReservation.Identity.Services;
using RestaurantReservation.Infra.Repositories;

namespace RestaurantReservation.WebApi.Extensions;

public static class DependencyInjector
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IReservationAppService, ReservationAppService>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<ITableRepository, TableRepository>();
    }
}
