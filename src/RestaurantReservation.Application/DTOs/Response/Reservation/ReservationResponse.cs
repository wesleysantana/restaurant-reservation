using NodaTime;

namespace RestaurantReservation.Application.DTOs.Response.Reservation;

public class ReservationResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid TableId { get; set; }
    public Instant StartsAt { get; set; }
    public Instant EndsAt { get; set; }
    public short NumberOfGuests { get; set; }
}