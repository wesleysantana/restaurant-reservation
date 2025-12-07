using Moq;
using NodaTime;
using RestaurantReservation.Application.DTOs.Request.Reservation;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Application.Services;
using RestaurantReservation.Application.Utils;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.Repositories;
using RestaurantReservation.Domain.ValueObjects;
using System.Reflection;

namespace RestaurantReservation.Test.Application;

public class ReservationAppServiceTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<ITableRepository> _tableRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ReservationAppService _service;

    public ReservationAppServiceTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _tableRepositoryMock = new Mock<ITableRepository>();

        _service = new ReservationAppService(
            _reservationRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _tableRepositoryMock.Object);
    }

    [Fact]
    public async Task MakeReservationAsync_ShouldFail_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)null);

        var request = new MakeReservationRequest
        {
            TableId = Guid.NewGuid(),
            StartsAt = Instant.FromDateTimeUtc(DateTime.UtcNow.AddHours(1)),
            EndsAt = Instant.FromDateTimeUtc(DateTime.UtcNow.AddHours(2)),
            NumberOfGuests = 2
        };

        // Act
        var result = await _service.MakeReservationAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        var error = Assert.Single(result.Errors);
        Assert.Equal(ProblemCode.UnauthorizedUser.ToString(), error.Metadata["Code"]);
    }

    [Fact]
    public async Task MakeReservationAsync_ShouldFail_WhenTableIsNotAvailable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tableId = Guid.NewGuid();

        var now = SystemClock.Instance.GetCurrentInstant();
        var startsAt = now.Plus(Duration.FromHours(1));
        var endsAt = now.Plus(Duration.FromHours(2));

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);
        
        var table = new Table(
            new Name("Mesa 01"),
            new Capacity(4),
            StatusTable.Disponivel);

        _tableRepositoryMock
            .Setup(x => x.GetByIdAsync(tableId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(table);

        var request = new MakeReservationRequest
        {
            TableId = tableId,
            StartsAt = startsAt,
            EndsAt = endsAt,
            NumberOfGuests = 2
        };

        _reservationRepositoryMock
            .Setup(x => x.IsTableAvailableAsync(
                tableId,
                startsAt,
                endsAt,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.MakeReservationAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        var error = Assert.Single(result.Errors);
        Assert.Equal(ProblemCode.TableUnavailable.ToString(), error.Metadata["Code"]);

        _tableRepositoryMock.Verify(x =>
            x.GetByIdAsync(tableId, It.IsAny<CancellationToken>()),
            Times.Once);

        _reservationRepositoryMock.Verify(x =>
            x.IsTableAvailableAsync(
                tableId,
                startsAt,
                endsAt,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _reservationRepositoryMock.Verify(x =>
            x.MakeReservationAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Instant>(),
                It.IsAny<Instant>(),
                It.IsAny<short>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task MakeReservationAsync_ShouldCreateReservation_WhenTableIsAvailableAndUserAuthenticated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tableId = Guid.NewGuid();

        var now = SystemClock.Instance.GetCurrentInstant();
        var startsAt = now.Plus(Duration.FromHours(1));
        var endsAt = now.Plus(Duration.FromHours(2));

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        var table = new Table(
            new Name("Mesa 01"),
            new Capacity(4),
            StatusTable.Disponivel);

        _tableRepositoryMock
            .Setup(x => x.GetByIdAsync(tableId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(table);

        var request = new MakeReservationRequest
        {
            TableId = tableId,
            StartsAt = startsAt,
            EndsAt = endsAt,
            NumberOfGuests = 2
        };

        _reservationRepositoryMock
            .Setup(x => x.IsTableAvailableAsync(
                tableId,
                startsAt,
                endsAt,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var reservation = new Reservation(
            userId,
            tableId,
            startsAt,
            endsAt,
            request.NumberOfGuests);

        _reservationRepositoryMock
            .Setup(x => x.MakeReservationAsync(
                userId,
                tableId,
                startsAt,
                endsAt,
                request.NumberOfGuests,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        var result = await _service.MakeReservationAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Value;

        Assert.Equal(reservation.Id, dto.Id);
        Assert.Equal(tableId, dto.TableId);
        Assert.Equal(userId, dto.CustomerId);
        Assert.Equal(startsAt, dto.StartsAt);
        Assert.Equal(endsAt, dto.EndsAt);
        Assert.Equal(request.NumberOfGuests, dto.NumberOfGuests);

        _tableRepositoryMock.Verify(x =>
            x.GetByIdAsync(tableId, It.IsAny<CancellationToken>()),
            Times.Once);

        _reservationRepositoryMock.Verify(x =>
            x.IsTableAvailableAsync(
                tableId,
                startsAt,
                endsAt,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _reservationRepositoryMock.Verify(x =>
            x.MakeReservationAsync(
                userId,
                tableId,
                startsAt,
                endsAt,
                request.NumberOfGuests,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldFail_WhenReservationNotFound()
    {
        // Arrange
        var request = new CancelReservationRequest
        {
            ReservationId = Guid.NewGuid()
        };

        _reservationRepositoryMock
            .Setup(x => x.GetReservationAsync(
                request.ReservationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        // Act
        var result = await _service.CancelReservationAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        var error = Assert.Single(result.Errors);
        Assert.Equal(ProblemCode.ReservationNotFound.ToString(), error.Metadata["Code"]);

        _reservationRepositoryMock.Verify(x =>
            x.CancelReservationAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldFail_WhenUserIsNotOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var reservation = new Reservation(
            ownerId,
            Guid.NewGuid(),
            now.Plus(Duration.FromHours(1)),
            now.Plus(Duration.FromHours(2)),
            2);

        var request = new CancelReservationRequest
        {
            ReservationId = reservation.Id
        };

        _reservationRepositoryMock
            .Setup(x => x.GetReservationAsync(
                request.ReservationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(otherUserId);

        // Act
        var result = await _service.CancelReservationAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        var error = Assert.Single(result.Errors);
        Assert.Equal(ProblemCode.ForbiddenReservationCancellation.ToString(), error.Metadata["Code"]);

        _reservationRepositoryMock.Verify(x =>
            x.CancelReservationAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldFail_WhenReservationAlreadyStarted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        var tableId = Guid.NewGuid();

        _currentUserServiceMock
            .SetupGet(x => x.UserId)
            .Returns(userId);

        var now = SystemClock.Instance.GetCurrentInstant();
        var startsAt = now.Minus(Duration.FromHours(1)); // já começou há 1h
        var endsAt = now.Plus(Duration.FromHours(1));

        var reservation = CreateReservationWithCustomDates(userId, tableId, startsAt, endsAt);

        _reservationRepositoryMock
            .Setup(x => x.GetReservationAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var request = new CancelReservationRequest { ReservationId = reservationId };

        // Act
        var result = await _service.CancelReservationAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        var error = Assert.Single(result.Errors);
        Assert.Equal(ProblemCode.InvalidReservationCancellation.ToString(), error.Metadata["Code"]);
    }


    [Fact]
    public async Task CancelReservationAsync_ShouldCancel_WhenReservationIsFutureAndUserIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);

        var reservation = new Reservation(
            userId,
            Guid.NewGuid(),
            now.Plus(Duration.FromHours(1)),
            now.Plus(Duration.FromHours(2)),
            2);

        var request = new CancelReservationRequest
        {
            ReservationId = reservation.Id
        };

        _reservationRepositoryMock
            .Setup(x => x.GetReservationAsync(
                request.ReservationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        // Act
        var result = await _service.CancelReservationAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _reservationRepositoryMock.Verify(x =>
            x.CancelReservationAsync(
                request.ReservationId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Reservation CreateReservationWithCustomDates(
        Guid userId,
        Guid tableId,
        Instant startsAt,
        Instant endsAt)
    {
        var type = typeof(Reservation);

        // Invoca o construtor privado sem parâmetros
        var ctor = type.GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        var reservation = (Reservation)ctor!.Invoke(null);

        // Seta as propriedades privadas
        type.GetProperty(nameof(Reservation.UserId))!
            .SetValue(reservation, userId);
        type.GetProperty(nameof(Reservation.TableId))!
            .SetValue(reservation, tableId);
        type.GetProperty(nameof(Reservation.StartsAt))!
            .SetValue(reservation, startsAt);
        type.GetProperty(nameof(Reservation.EndsAt))!
            .SetValue(reservation, endsAt);
        type.GetProperty(nameof(Reservation.Status))!
            .SetValue(reservation, StatusReservation.Ativo);
        type.GetProperty(nameof(Reservation.NumberOfGuests))!
            .SetValue(reservation, (short)2);

        return reservation;
    }
}