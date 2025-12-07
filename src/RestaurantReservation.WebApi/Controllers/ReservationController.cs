using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantReservation.Application.DTOs.Request.Reservation;
using RestaurantReservation.Application.DTOs.Response.Reservation;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.WebApi.Extensions;
using RestaurantReservation.WebApi.Localization;

namespace RestaurantReservation.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IReservationAppService _reservationAppService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ReservationController(IReservationAppService reservationAppService, IStringLocalizer<SharedResource> localizer)
    {
        _reservationAppService = reservationAppService;
        _localizer = localizer;
    }

    [HttpPost("make-reservation")]
    public async Task<ActionResult<ReservationResponse>> MakeReservation([FromBody] MakeReservationRequest request, CancellationToken ct)
    {
        // Condicional desnecessário em produção, mas útil para testes unitários
        if (!ModelState.IsValid)
            return BadRequest();

        var result = await _reservationAppService.MakeReservationAsync(request, ct);

        return (ActionResult)result.ToActionResult(this, _localizer, dto => Created($"/api/reservation/{dto.Id}", dto));
    }

    [HttpDelete]
    public async Task<ActionResult> CancelReservation([FromBody] CancelReservationRequest request, CancellationToken ct)
    {
        // Condicional desnecessário em produção, mas útil para testes unitários
        if (!ModelState.IsValid)
            return BadRequest();

        var result = await _reservationAppService.CancelReservationAsync(request, ct);

        return (ActionResult)result.ToActionResult(this, _localizer, () => NoContent());
    }
}