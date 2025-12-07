using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using RestaurantReservation.Application.DTOs.Request.User;
using RestaurantReservation.Application.DTOs.Response.User;
using RestaurantReservation.Application.Extensions;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Application.Utils;
using RestaurantReservation.Test.WebApi.Helpers;
using RestaurantReservation.WebApi.Controllers;
using RestaurantReservation.WebApi.Localization;
using System.Net;

namespace RestaurantReservation.Test.WebApi;

public class UserControllerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UserControllerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _localizer = new FakeStringLocalizer();
    }

    private UserController CreateController()
    {
        return new UserController(_identityServiceMock.Object, _localizer);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenResultIsSuccess()
    {
        var controller = CreateController();

        var request = new RegisterUserRequest
        {
            Email = "user@test.com",
            Password = "StrongP@ssw0rd",
            PasswordConfirmation = "StrongP@ssw0rd"
        };

        var response = new RegisterUserResponse(true);

        _identityServiceMock
            .Setup(x => x.RegisterUser(request))
            .ReturnsAsync(Result.Ok(response));

        var result = await controller.Register(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<RegisterUserResponse>(okResult.Value);
        Assert.True(value.Success);
    }

    [Fact]
    public async Task Register_ShouldReturnProblemDetails_WhenResultFails()
    {
        var controller = CreateController();

        var request = new RegisterUserRequest
        {
            Email = "user@test.com",
            Password = "123",
            PasswordConfirmation = "123"
        };

        var error = new Error("Invalid password");
        _identityServiceMock
            .Setup(x => x.RegisterUser(request))
            .ReturnsAsync(Result.Fail<RegisterUserResponse>(error));

        var result = await controller.Register(request);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Business rule violation", problem.Title);
        Assert.Contains("Invalid password", problem.Detail);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Email", "Required");

        var request = new UserLoginRequest();

        var actionResult = await controller.Login(request);

        Assert.IsType<BadRequestResult>(actionResult.Result);
        _identityServiceMock.Verify(x => x.Login(It.IsAny<UserLoginRequest>()), Times.Never);       
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenResultIsSuccess()
    {
        var controller = CreateController();

        var request = new UserLoginRequest
        {
            Email = "user@test.com",
            Password = "StrongP@ssw0rd"
        };

        var response = new UserLoginResponse("access", "refresh");

        _identityServiceMock
            .Setup(x => x.Login(request))
            .ReturnsAsync(Result.Ok(response));

        var actionResult = await controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var value = Assert.IsType<UserLoginResponse>(okResult.Value);
        Assert.Equal("access", value.AccessToken);
        Assert.Equal("refresh", value.RefreshToken);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenResultFailsWithUnauthorizedUser()
    {
        var controller = CreateController();

        var request = new UserLoginRequest
        {
            Email = "user@test.com",
            Password = "wrong"
        };

        var error = new Error("Usuário ou senha estão incorretos")
            .WithCode(ProblemCode.UnauthorizedUser.ToString());

        _identityServiceMock
            .Setup(x => x.Login(request))
            .ReturnsAsync(Result.Fail<UserLoginResponse>(error));

        var actionResult = await controller.Login(request);

        // ResultExtensions vai mapear UnauthorizedUser => 401
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Business rule violation", problem.Title);
        Assert.Contains("Usuário ou senha estão incorretos", problem.Detail);
    }

    [Fact]
    public async Task RefreshLogin_ShouldReturnBadRequest_WhenRequestInvalid()
    {
        var controller = CreateController();
        var request = new RefreshLoginRequest { RefreshToken = "" };

        var actionResult = await controller.RefreshLogin(request);

        Assert.IsType<BadRequestResult>(actionResult.Result);
        _identityServiceMock.Verify(x => x.RefreshLogin(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RefreshLogin_ShouldReturnOk_WhenResultIsSuccess()
    {
        var controller = CreateController();
        var request = new RefreshLoginRequest { RefreshToken = "valid.refresh.token" };

        var response = new UserLoginResponse("new-access", "new-refresh");

        _identityServiceMock
            .Setup(x => x.RefreshLogin(request.RefreshToken))
            .ReturnsAsync(Result.Ok(response));

        var actionResult = await controller.RefreshLogin(request);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var value = Assert.IsType<UserLoginResponse>(okResult.Value);
        Assert.Equal("new-access", value.AccessToken);
    }

    [Fact]
    public async Task RefreshLogin_ShouldReturnUnauthorized_WhenResultFailsWithUnauthorizedUser()
    {
        var controller = CreateController();
        var request = new RefreshLoginRequest { RefreshToken = "invalid" };

        var error = new Error("Refresh token inválido")
            .WithCode(ProblemCode.UnauthorizedUser.ToString());

        _identityServiceMock
            .Setup(x => x.RefreshLogin(request.RefreshToken))
            .ReturnsAsync(Result.Fail<UserLoginResponse>(error));

        var actionResult = await controller.RefreshLogin(request);

        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Refresh token inválido", problem.Detail);
    }
}