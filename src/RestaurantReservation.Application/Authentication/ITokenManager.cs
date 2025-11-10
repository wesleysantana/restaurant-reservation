using RestaurantReservation.Domain.Domains;

namespace RestaurantReservation.Application.Authentication;

public interface ITokenManager
{
    string GerarToken(User user);

    string GerarRefreshToken(User user);

    Task<(bool isValid, string? userEmail)> ValidateTokenAsync(string token);
}