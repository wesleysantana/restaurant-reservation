using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.DTOs.Request.Reservation;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Application.Utils;
using RestaurantReservation.WebApi.Extensions;

namespace RestaurantReservation.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservationsController : ControllerBase
{
    private readonly IReservationAppService _reservationAppService;

    public ReservationsController(IReservationAppService reservationAppService)
    {
        _reservationAppService = reservationAppService;
    }

    [HttpPost]
    public async Task<IActionResult> MakeReservation(
        [FromBody] MakeReservationRequest request,
        CancellationToken cancellationToken)
    {
        /*
        var result = await _reservationAppService.MakeReservationAsync(request, cancellationToken);
        
        if (result.IsSuccess)
            return Ok(result.Value);
        
        var firstError = result.Errors.First();

        // Se for conflito de mesa, retornar 409 Conflict
        var statusCode = firstError.Metadata.TryGetValue(ErrorMetadataKeys.Code, out var code) &&
                         code?.ToString() == ProblemCode.TableUnavailable.ToString()
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status400BadRequest;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = "Reservation could not be created",
            Detail = string.Join("; ", result.Errors.Select(e => e.Message)),
            Type = "https://httpstatuses.com/" + statusCode
        };

        problem.Extensions["errors"] = result.Errors
            .Select(e => new
            {
                e.Message,
                Code = e.Metadata.TryGetValue("Code", out var c) ? c : null
            });

        return StatusCode(statusCode, problem);
        */

        var result = await _reservationAppService.MakeReservationAsync(request, cancellationToken);

        return result.ToActionResult(this);

    }
}