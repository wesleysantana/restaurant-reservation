using NodaTime;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.Exceptions;

namespace RestaurantReservation.Test.Domain;

public class ReservationTests
{
    [Fact]
    public void Ctor_WithEmptyUserId_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.Empty;
        var tableId = Guid.NewGuid();
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var startsAt = now.Plus(Duration.FromHours(1));
        var endsAt = startsAt.Plus(Duration.FromHours(2));

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            new Reservation(userId, tableId, startsAt, endsAt, 2));
    }

    [Fact]
    public void Ctor_WithEndsBeforeStarts_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var startsAt = now.Plus(Duration.FromHours(2));
        var endsAt = now.Plus(Duration.FromHours(1));

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            new Reservation(userId, tableId, startsAt, endsAt, 2));
    }

    [Fact]
    public void Ctor_WithNegativeGuests_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var startsAt = now.Plus(Duration.FromHours(1));
        var endsAt = startsAt.Plus(Duration.FromHours(1));

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            new Reservation(userId, tableId, startsAt, endsAt, -1));
    }

    [Fact]
    public void IsActive_ShouldBeTrue_WhenStatusIsAtivo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var startsAt = now.Plus(Duration.FromHours(1));
        var endsAt = startsAt.Plus(Duration.FromHours(1));

        var reservation = new Reservation(userId, tableId, startsAt, endsAt, 2);

        // Act
        var isActive = reservation.IsActive;

        // Assert
        Assert.True(isActive);
        Assert.Equal(StatusReservation.Ativo, reservation.Status);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelado_WhenActive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var startsAt = now.Plus(Duration.FromHours(1));
        var endsAt = startsAt.Plus(Duration.FromHours(1));

        var reservation = new Reservation(userId, tableId, startsAt, endsAt, 2);

        // Act
        reservation.Cancel();

        // Assert
        Assert.Equal(StatusReservation.Cancelado, reservation.Status);
        Assert.False(reservation.IsActive);
    }

    [Fact]
    public void Cancel_ShouldBeIdempotent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var startsAt = now.Plus(Duration.FromHours(1));
        var endsAt = startsAt.Plus(Duration.FromHours(1));

        var reservation = new Reservation(userId, tableId, startsAt, endsAt, 2);

        // Act
        reservation.Cancel();
        var firstStatus = reservation.Status;

        // Chama de novo
        reservation.Cancel();
        var secondStatus = reservation.Status;

        // Assert
        Assert.Equal(StatusReservation.Cancelado, firstStatus);
        Assert.Equal(StatusReservation.Cancelado, secondStatus);
    }
}