using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestaurantReservation.Application.DTOs.Request.User;
using RestaurantReservation.Application.DTOs.Response.User;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Identity.Configurations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace RestaurantReservation.Identity.Services;

public class IdentityService : IIdentityService
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtOptions _jwtOptions;

    public IdentityService(SignInManager<IdentityUser> signInManager,
                           UserManager<IdentityUser> userManager,
                           IOptions<JwtOptions> jwtOptions)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<RegisterUserResponse> RegisterUser(RegisterUserRequest userRegistration)
    {
        var identityUser = new IdentityUser
        {
            UserName = userRegistration.Email,
            Email = userRegistration.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(identityUser, userRegistration.Password);
        if (result.Succeeded)
            await _userManager.SetLockoutEnabledAsync(identityUser, false); // Desabilita o lockout para o usuário recém-criado

        var registerUserResponse = new RegisterUserResponse(result.Succeeded);
        if (!result.Succeeded && result.Errors.Any())
            registerUserResponse.AddErrors(result.Errors.Select(r => r.Description));

        return registerUserResponse;
    }

    public async Task<UserLoginResponse> Login(UserLoginRequest userLogin)
    {
        var user = await _userManager.FindByEmailAsync(userLogin.Email);

        var userLoginResponse = new UserLoginResponse();
        if (user is null)
        {
            userLoginResponse.AddError("Usuário ou senha estão incorretos");
            return userLoginResponse;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, userLogin.Password, true);
        if (result.Succeeded)
            return await GenerateCredentials(user);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                userLoginResponse.AddError("Essa conta está bloqueada");
            else if (result.IsNotAllowed)
                userLoginResponse.AddError("Essa conta não tem permissão para fazer login");
            else if (result.RequiresTwoFactor)
                userLoginResponse.AddError("É necessário confirmar o login no seu segundo fator de autenticação");
            else
                userLoginResponse.AddError("Usuário ou senha estão incorretos");
        }

        return userLoginResponse;
    }

    public async Task<UserLoginResponse> RefreshLogin(string refreshToken)
    {
        var response = new UserLoginResponse();

        var tokenHandler = new JwtSecurityTokenHandler();
        ClaimsPrincipal principal;

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _jwtOptions.SigningCredentials.Key,
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                jwtToken.Claims.FirstOrDefault(c => c.Type == "typ")?.Value != "refresh")
            {
                response.AddError("Refresh token inválido");
                return response;
            }
        }
        catch (SecurityTokenExpiredException)
        {
            response.AddError("Refresh token expirado");
            return response;
        }
        catch
        {
            response.AddError("Refresh token inválido");
            return response;
        }

        // Pega o userId (sub) das claims
        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            response.AddError("Usuário não encontrado");
            return response;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            response.AddError("Usuário não encontrado");
            return response;
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            response.AddError("Essa conta está bloqueada");
            return response;
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            response.AddError("Essa conta precisa confirmar seu e-mail antes de realizar o login");
            return response;
        }
        
        return await GenerateCredentials(user);
    }

    private async Task<UserLoginResponse> GenerateCredentials(IdentityUser user)
    {
        var accessTokenClaims = await GetClaims(user, isAccessToken: true);
        var refreshTokenClaims = await GetClaims(user, isAccessToken: false);

        var expirationDateAccessToken = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationInMinutes);
        var expirationDateRefreshToken = DateTime.UtcNow.AddMinutes(_jwtOptions.RefreshTokenExpirationInMinutes);

        var accessToken = GenerateToken(accessTokenClaims, expirationDateAccessToken);
        var refreshToken = GenerateToken(refreshTokenClaims, expirationDateRefreshToken);

        return new UserLoginResponse(accessToken, refreshToken);
    }

    private string GenerateToken(IEnumerable<Claim> claims, DateTime dateExpires)
    {
        var jwt = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.Now,
            expires: dateExpires,
            signingCredentials: _jwtOptions.SigningCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private async Task<IList<Claim>> GetClaims(IdentityUser user, bool isAccessToken)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
            new("typ", isAccessToken ? "access" : "refresh")
        };

        if (isAccessToken)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            claims.AddRange(userClaims);           

            foreach (var role in roles)
                claims.Add(new Claim("role", role));           
        }

        return claims;
    }
}