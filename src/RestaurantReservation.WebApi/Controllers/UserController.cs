using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantReservation.Application.DTOs.Request.User;
using RestaurantReservation.Application.DTOs.Response.User;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.WebApi.Extensions;
using RestaurantReservation.WebApi.Localization;

namespace RestaurantReservation.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private IIdentityService _identityService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UserController(IIdentityService identityService, IStringLocalizer<SharedResource> localizer)
    {
        _identityService = identityService;
        _localizer = localizer;
    }

    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest registerUser)
    {
        // Condicional desnecessário em produção, mas útil para testes unitários
        if (!ModelState.IsValid)
            return BadRequest();

        var result = await _identityService.RegisterUser(registerUser);

        return result.ToActionResult(this, _localizer, dto => Ok(dto));
    }

    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpPost("login")]
    public async Task<ActionResult<RegisterUserResponse>> Login(UserLoginRequest userLogin)
    {
        // Condicional desnecessário em produção, mas útil para testes unitários
        if (!ModelState.IsValid)
            return BadRequest();

        var result = await _identityService.Login(userLogin);

        return (ActionResult)result.ToActionResult(this, _localizer, dto => Ok(dto));
    }

    [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpPost("refresh-login")]
    public async Task<ActionResult<UserLoginResponse>> RefreshLogin([FromBody] RefreshLoginRequest request)
    {
        // Condicional desnecessário em produção, mas útil para testes unitários
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest();
        var result = await _identityService.RefreshLogin(request.RefreshToken);

        return (ActionResult)result.ToActionResult(this, _localizer, dto => Ok(dto));
    }
}