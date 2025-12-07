using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NodaTime;
using RestaurantReservation.Application.DTOs.Request.Reservation;
using RestaurantReservation.Application.DTOs.Response.Reservation;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Application.Utils;
using RestaurantReservation.WebApi.Controllers;
using RestaurantReservation.WebApi.Localization;
using RestaurantReservation.Application.Extensions;

namespace RestaurantReservation.Test.WebApi;

public class ReservationControllerTests
{
    private readonly Mock<IReservationAppService> _reservationAppServiceMock;
    private readonly Mock<IStringLocalizer<SharedResource>> _localizerMock;
    private readonly ReservationController _controller;

    public ReservationControllerTests()
    {
        _reservationAppServiceMock = new Mock<IReservationAppService>();

        _localizerMock = new Mock<IStringLocalizer<SharedResource>>();
        _localizerMock
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key, resourceNotFound: true));
        _localizerMock
            .Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, key, resourceNotFound: true));

        _controller = new ReservationController(
            _reservationAppServiceMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task MakeReservation_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("TableId", "Required");

        var request = new MakeReservationRequest(); // tanto faz o conteúdo

        // Act
        var actionResult = await _controller.MakeReservation(request, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestResult>(actionResult.Result);
        _reservationAppServiceMock.Verify(
            s => s.MakeReservationAsync(It.IsAny<MakeReservationRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task MakeReservation_ShouldReturnOk_WhenServiceReturnsSuccess()
    {
        // Arrange
        var request = new MakeReservationRequest
        {
            TableId = Guid.NewGuid(),
            NumberOfGuests = 2,
            StartsAt = Instant.FromDateTimeUtc(DateTime.UtcNow.AddHours(1)),
            EndsAt = Instant.FromDateTimeUtc(DateTime.UtcNow.AddHours(2)),
        };

        var dto = new ReservationResponse
        {
            Id = Guid.NewGuid(),
            TableId = request.TableId,
            CustomerId = Guid.NewGuid(),
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            NumberOfGuests = request.NumberOfGuests            
        };

        _reservationAppServiceMock
            .Setup(s => s.MakeReservationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(dto));

        // Act
        var actionResult = await _controller.MakeReservation(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<CreatedResult>(actionResult.Result);
        var value = Assert.IsType<ReservationResponse>(okResult.Value);

        Assert.Equal(dto.Id, value.Id);
        Assert.Equal(dto.TableId, value.TableId);

        _reservationAppServiceMock.Verify(
            s => s.MakeReservationAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MakeReservation_ShouldReturnConflict_WhenTableUnavailable()
    {
        // Arrange
        var request = new MakeReservationRequest
        {
            TableId = Guid.NewGuid(),
            NumberOfGuests = 4
        };

        var error = new Error("Table unavailable")
            .WithCode(ProblemCode.TableUnavailable.ToString());

        _reservationAppServiceMock
            .Setup(s => s.MakeReservationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<ReservationResponse>(error));

        // Act
        var actionResult = await _controller.MakeReservation(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Table unavailable", problem.Detail);

        _reservationAppServiceMock.Verify(
            s => s.MakeReservationAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelReservation_ShouldReturnBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("ReservationId", "Required");

        var request = new CancelReservationRequest();

        // Act
        var actionResult = await _controller.CancelReservation(request, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestResult>(actionResult);
        _reservationAppServiceMock.Verify(
            s => s.CancelReservationAsync(It.IsAny<CancelReservationRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CancelReservation_ShouldReturnNoContent_WhenServiceReturnsSuccess()
    {
        // Arrange
        var request = new CancelReservationRequest
        {
            ReservationId = Guid.NewGuid()
        };

        _reservationAppServiceMock
            .Setup(s => s.CancelReservationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var actionResult = await _controller.CancelReservation(request, CancellationToken.None);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(actionResult);

        _reservationAppServiceMock.Verify(
            s => s.CancelReservationAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelReservation_ShouldReturnProperStatus_WhenServiceFails()
    {
        // Arrange
        var request = new CancelReservationRequest
        {
            ReservationId = Guid.NewGuid()
        };

        var error = new Error("User cannot cancel this reservation")
            .WithCode(ProblemCode.ForbiddenReservationCancellation.ToString());

        _reservationAppServiceMock
            .Setup(s => s.CancelReservationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(error));

        // Act
        var actionResult = await _controller.CancelReservation(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("User cannot cancel this reservation", problem.Detail);
    }
}

