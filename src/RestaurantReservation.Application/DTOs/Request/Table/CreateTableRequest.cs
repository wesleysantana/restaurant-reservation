using RestaurantReservation.Application.Utils;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Request.Table;

public class CreateTableRequest
{
    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = MessagesDataAnnotations.Range)]
    public short Capacity { get; set; }
}