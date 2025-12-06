using FluentResults;
using RestaurantReservation.Application.DTOs.Request.Reservation;
using RestaurantReservation.Application.DTOs.Response.Reservation;
using RestaurantReservation.Application.Extensions;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Application.Utils;
using RestaurantReservation.Domain.Repositories;

namespace RestaurantReservation.Application.Services;

public class ReservationAppService : IReservationAppService
{
    private readonly IReservationRepository _reservationRepository;

    public ReservationAppService(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<Result<ReservationResponse>> MakeReservationAsync(
        MakeReservationRequest request,
        CancellationToken cancellationToken)
    {
        var isAvailable = await _reservationRepository.IsTableAvailableAsync(
            request.TableId,
            request.StartsAt,
            request.EndsAt,
            cancellationToken);

        if (!isAvailable)
        {
            return Result
                .Fail<ReservationResponse>(new Error("Table is not available for the selected time.")
                .WithCode(ProblemCode.TableUnavailable.ToString()));
        }

        var reservation = await _reservationRepository.MakeReservationAsync(
            request.CustomerId,
            request.TableId,
            request.StartsAt,
            request.EndsAt,
            request.NumberOfGuests,
            cancellationToken);

        var dto = new ReservationResponse
        {
            Id = reservation.Id,
            TableId = reservation.TableId,
            CustomerId = reservation.UserId,
            StartsAt = reservation.StartsAt,
            EndsAt = reservation.EndsAt,
            NumberOfGuests = reservation.NumberOfGuests
        };

        return Result.Ok(dto);
    }

    public async Task<Result> CancelReservationAsync(CancelReservationRequest request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository
            .GetReservationAsync(request.ReservationId, cancellationToken);

        if (reservation is null)
        {
            return Result
                .Fail(new Error("Reservation not found.")
                .WithCode(ProblemCode.ReservationNotFound.ToString()));
        }

        // aqui você pode colocar regra de negócio:
        // - não permitir cancelar reserva que já começou/passou, etc.

        await _reservationRepository.CancelReservationAsync(
            request.ReservationId,
            cancellationToken);

        return Result.Ok();
    }

}