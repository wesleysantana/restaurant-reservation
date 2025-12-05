using FluentResults;
using RestaurantReservation.Application.DTOs.Request.Reservation;
using RestaurantReservation.Application.DTOs.Response.Reservation;

namespace RestaurantReservation.Application.Interfaces;

public interface IReservationAppService
{
    Task<Result<ReservationResponse>> MakeReservationAsync(
        MakeReservationRequest request,
        CancellationToken cancellationToken);
}