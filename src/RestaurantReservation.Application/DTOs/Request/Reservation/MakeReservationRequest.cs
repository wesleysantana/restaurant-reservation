using NodaTime;

namespace RestaurantReservation.Application.DTOs.Request.Reservation;

public class MakeReservationRequest
{
    public Guid CustomerId { get; set; }
    public Guid TableId { get; set; }
    public Instant StartsAt { get; set; }
    public Instant EndsAt { get; set; }
    public short NumberOfGuests { get; set; }
}