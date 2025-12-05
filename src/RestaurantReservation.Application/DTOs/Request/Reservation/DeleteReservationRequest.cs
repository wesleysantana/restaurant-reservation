using RestaurantReservation.Application.Utils;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Request.Reservation;

public class DeleteReservationRequest
{
    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    public Guid ReservationId { get; set; }
}