using NodaTime;
using RestaurantReservation.Application.Utils;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Request.Reservation;

public class MakeReservationRequest
{
    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    public Guid TableId { get; set; }

    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    public Instant StartsAt { get; set; }

    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    public Instant EndsAt { get; set; }

    [Range(1, 100, ErrorMessage = MessagesDataAnnotations.Range)]
    public short NumberOfGuests { get; set; }
}