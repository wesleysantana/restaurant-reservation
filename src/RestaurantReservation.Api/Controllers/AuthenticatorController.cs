using Microsoft.AspNetCore.Mvc;

namespace RestaurantReservation.Api.Controllers;

[Route("api/users")]
[ApiController]
public class AuthenticatorController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register()
    {
        return (IActionResult)Task.CompletedTask;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login()
    {
        return (IActionResult)Task.CompletedTask;
    }
}