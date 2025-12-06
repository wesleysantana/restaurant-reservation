using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantReservation.Application.DTOs.Request.Reservation;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.WebApi.Extensions;
using RestaurantReservation.WebApi.Localization;

namespace RestaurantReservation.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservationsController : ControllerBase
{
    private readonly IReservationAppService _reservationAppService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ReservationsController(IReservationAppService reservationAppService, IStringLocalizer<SharedResource> localizer)
    {
        _reservationAppService = reservationAppService;
        _localizer = localizer;
    }

    [HttpPost("make-reservation")]
    public async Task<IActionResult> MakeReservation([FromBody] MakeReservationRequest request, CancellationToken ct)
    {
        var result = await _reservationAppService.MakeReservationAsync(request, ct);

        return result.ToActionResult(this, _localizer, value => Created());
    }

    [HttpDelete]
    public async Task<IActionResult> CancelReservation([FromBody] CancelReservationRequest request, CancellationToken ct)
    {
        var result = await _reservationAppService.CancelReservationAsync(request, ct);

        return result.ToActionResult(this, _localizer, () => NoContent());
    }
}