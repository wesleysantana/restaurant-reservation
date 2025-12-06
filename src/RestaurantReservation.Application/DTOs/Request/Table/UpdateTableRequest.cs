using RestaurantReservation.Application.Utils;
using RestaurantReservation.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Request.Table;

public class UpdateTableRequest
{
    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    public Guid Id { get; set; }

    [StringLength(255, ErrorMessage = MessagesDataAnnotations.StringLength, MinimumLength = 3)]
    public string? Name { get; set; }

    [Range(1, 100, ErrorMessage = MessagesDataAnnotations.Range)]
    public short? Capacity { get; set; }

    public StatusTable? Status { get; set; }
}