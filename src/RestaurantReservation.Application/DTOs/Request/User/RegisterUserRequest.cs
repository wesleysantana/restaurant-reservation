using RestaurantReservation.Application.Utils;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Request.User;

public class RegisterUserRequest
{
    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    [EmailAddress(ErrorMessage = MessagesDataAnnotations.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    [StringLength(50, ErrorMessage = MessagesDataAnnotations.StringLength, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Compare(nameof(Password), ErrorMessage = "A senha e a senha de confirmação devem ser iguais")]
    public string PasswordConfirmation { get; set; } = string.Empty;
}