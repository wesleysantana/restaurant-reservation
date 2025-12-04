using Microsoft.IdentityModel.Tokens;

namespace RestaurantReservation.Identity.Configurations;

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public SigningCredentials SigningCredentials { get; set; } = null!;
    public int AccessTokenExpirationInMinutes { get; set; }
    public int RefreshTokenExpirationInMinutes { get; set; }
}