using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Application.Services;
using RestaurantReservation.Domain.Repositories;
using RestaurantReservation.Identity.Services;
using RestaurantReservation.Infra.Repositories;
using RestaurantReservation.WebApi.Services;

namespace RestaurantReservation.WebApi.Extensions;

public static class DependencyInjector
{
    public static void RegisterServices(this IServiceCollection services)
    {
        #region Services

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IReservationAppService, ReservationAppService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITableAppService, TableAppService>();

        #endregion Services

        #region Repositories

        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<ITableRepository, TableRepository>();

        #endregion Repositories
    }
}