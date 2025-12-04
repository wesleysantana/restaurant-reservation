namespace RestaurantReservation.Application.DTOs.Request.User;

public class RefreshLoginRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}