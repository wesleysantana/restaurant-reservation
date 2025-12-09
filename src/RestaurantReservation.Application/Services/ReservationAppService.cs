using FluentResults;
using NodaTime;
using RestaurantReservation.Application.DTOs.Request.Reservation;
using RestaurantReservation.Application.DTOs.Response.Reservation;
using RestaurantReservation.Application.Extensions;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Application.Utils;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.Repositories;

namespace RestaurantReservation.Application.Services;

public class ReservationAppService : IReservationAppService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBusinessHoursAppService _businessHoursService;
    private readonly ITableRepository _tableRepository;


    public ReservationAppService(
        IReservationRepository reservationRepository,
        ICurrentUserService currentUserService,
        IBusinessHoursAppService businessHoursService,
        ITableRepository tableRepository)
    {
        _reservationRepository = reservationRepository;
        _currentUserService = currentUserService;
        _businessHoursService = businessHoursService;
        _tableRepository = tableRepository;
    }

    public async Task<Result<ReservationResponse>> MakeReservationAsync(
        MakeReservationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Result.Fail<ReservationResponse>(
                new Error("User not authenticated.")
                    .WithCode(ProblemCode.UnauthorizedUser.ToString()));
        }

        // Verifica dia/horário de funcionamento       
        if (!await _businessHoursService.IsOpenAsync(request.StartsAt, request.EndsAt, cancellationToken))
        {
            return Result
                .Fail<ReservationResponse>(new Error("Reserva fora do horário de funcionamento.")
                    .WithCode(ProblemCode.InvalidBusinessHours.ToString()));
        }


        // 1) Buscar mesa
        var table = await _tableRepository.GetByIdAsync(request.TableId, cancellationToken);
        if (table is null)
        {
            return Result
                .Fail<ReservationResponse>(new Error("Table not found.")
                .WithCode(ProblemCode.TableUnavailable.ToString()));
        }

        // 2) Verificar se mesa está inativa
        if (table.Status == StatusTable.Inativa)
        {
            return Result
                .Fail<ReservationResponse>(new Error("Table is inactive.")
                .WithCode(ProblemCode.TableUnavailable.ToString()));
        }

        // 3) Validar capacidade da mesa
        if (request.NumberOfGuests > table.Capacity.Value)
        {
            return Result
                .Fail<ReservationResponse>(new Error("Number of guests exceeds table capacity.")
                .WithCode(ProblemCode.TableUnavailable.ToString()));
        }

        // 4) Verificar disponibilidade pelo intervalo
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

        // 5) Criar reserva
        var reservation = await _reservationRepository.MakeReservationAsync(
            userId.Value,
            request.TableId,
            request.StartsAt,
            request.EndsAt,
            request.NumberOfGuests,
            cancellationToken);

        // 6) Atualizar status da mesa para RESERVADA
        if (table.Status != StatusTable.Reservada)
        {
            table.Update(status: StatusTable.Reservada);
            await _tableRepository.UpdateAsync(table, cancellationToken);
        }

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

        var userId = _currentUserService.UserId;
        if (userId is null || reservation.UserId != userId.Value)
        {
            return Result
                .Fail(new Error("You cannot cancel a reservation that is not yours.")

                .WithCode(ProblemCode.ForbiddenReservationCancellation.ToString()));
        }

        if (reservation.StartsAt <= Instant.FromDateTimeUtc(DateTime.UtcNow))
        {
            return Result
                .Fail(new Error("Cannot cancel a reservation that has already started or passed.")
                .WithCode(ProblemCode.InvalidReservationCancellation.ToString()));
        }

        // Buscar mesa associada à reserva
        var table = await _tableRepository.GetByIdAsync(reservation.TableId, cancellationToken);

        // Cancelar reserva
        await _reservationRepository.CancelReservationAsync(request.ReservationId, cancellationToken);

        // Ao cancelar, liberar a mesa => status DISPONÍVEL
        if (table is not null && table.Status != StatusTable.Disponivel)
        {
            table.Update(status: StatusTable.Disponivel);
            await _tableRepository.UpdateAsync(table, cancellationToken);
        }       

        return Result.Ok();
    }
}