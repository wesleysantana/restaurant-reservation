using RestaurantReservation.Application.Utils;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Request.User;

public class UserLoginRequest
{
    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    [EmailAddress(ErrorMessage = MessagesDataAnnotations.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    public string Password { get; set; } = string.Empty;
}