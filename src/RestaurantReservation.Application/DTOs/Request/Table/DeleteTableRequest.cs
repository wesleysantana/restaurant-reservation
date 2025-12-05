using RestaurantReservation.Application.Utils;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Request.Table;

public class DeleteTableRequest
{
    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    public Guid TableId { get; set; }
}