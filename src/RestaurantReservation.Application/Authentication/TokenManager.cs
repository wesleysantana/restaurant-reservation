using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestaurantReservation.Domain.Domains;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestaurantReservation.Application.Authentication;

internal sealed class TokenManager : ITokenManager
{
    private readonly IConfiguration _configuration;

    public TokenManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GerarToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(jwtSettings["SecretKey"] ?? string.Empty));

        // Informações contidas no token
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Email.Value),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        };
        

        // Tempo de expiração.
        var tempoExpiracaoInMinutes = Convert.ToInt16(jwtSettings["ExpirationTimeInMinutes"]);

        // Montando token
        var token = new JwtSecurityToken(
            issuer: jwtSettings.["Issuer"], // Quem emite
            audience: jwtSettings["Audience"], // Quem consume
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(5),
            signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GerarRefreshToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(jwtSettings["SecretKey"] ?? string.Empty));

        // Informações contidas no token
        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email.Value),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // Tempo de expiração.
        var tempoExpiracaoInMinutes = Convert.ToInt16(jwtSettings["RefreshExpirationTimeInMinutes"]);

        // Montando token
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"], // Quem emite
            audience: jwtSettings["Audience"], // Quem consume
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(tempoExpiracaoInMinutes),
            signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<(bool isValid, string? userEmail)> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, null);

        var tokenParameters = TokenHelpers.GetTokenValidationParameters(_configuration);
        var validTokenResult = await new JwtSecurityTokenHandler().ValidateTokenAsync(token, tokenParameters);

        if (!validTokenResult.IsValid)
            return (false, null);

        var userName = validTokenResult
            .Claims.FirstOrDefault(c => c.Key == ClaimTypes.NameIdentifier).Value as string;

        return (true, userName);
    }
}