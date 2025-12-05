using RestaurantReservation.Application.Utils;
using RestaurantReservation.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Request.Table;

public class AlterStatusTableRequest
{
    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    public Guid TableId { get; set; }

    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    public StatusTable StatusTable { get; set; }
}