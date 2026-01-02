using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RestaurantReservation.Identity.Configurations;
using System.Text;

namespace RestaurantReservation.WebApi.Extensions;

public static class AuthenticationSetup
{
    public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("JwtOptions");

        var securityKeyValue = jwtSection["SecurityKey"];
        if (string.IsNullOrWhiteSpace(securityKeyValue))
            throw new InvalidOperationException(
                "JwtOptions:SecurityKey não configurado. " +
                "Configure em appsettings.Development.json (local) ou via env var JwtOptions__SecurityKey (Docker)."
            );

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKeyValue));

        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = jwtSection[nameof(JwtOptions.Issuer)]!;
            options.Audience = jwtSection[nameof(JwtOptions.Audience)]!;
            options.SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
            options.AccessTokenExpirationInMinutes = int.Parse(jwtSection[nameof(JwtOptions.AccessTokenExpirationInMinutes)] ?? "0");
            options.RefreshTokenExpirationInMinutes = int.Parse(jwtSection[nameof(JwtOptions.RefreshTokenExpirationInMinutes)] ?? "0");
        });

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
        });

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuration.GetSection("JwtOptions:Issuer").Value,

            ValidateAudience = true,
            ValidAudience = configuration.GetSection("JwtOptions:Audience").Value,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,

            RequireExpirationTime = true,
            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = tokenValidationParameters;

            // Adiciona o evento para manipular tokens expirados
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        // Adiciona um cabeçalho personalizado indicando que o token expirou
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });
    }
}