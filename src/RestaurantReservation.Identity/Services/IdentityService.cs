using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestaurantReservation.Application.DTOs.Request.User;
using RestaurantReservation.Application.DTOs.Response.User;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Application.Utils;
using RestaurantReservation.Identity.Configurations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using RestaurantReservation.Application.Extensions;


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

    public async Task<Result<RegisterUserResponse>> RegisterUser(RegisterUserRequest userRegistration)
    {
        var identityUser = new IdentityUser
        {
            UserName = userRegistration.Email,
            Email = userRegistration.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(identityUser, userRegistration.Password);
        if (result.Succeeded)
        {
            await _userManager.SetLockoutEnabledAsync(identityUser, false);

            var response = new RegisterUserResponse(true);
            return Result.Ok(response);
        }

        if (!result.Errors.Any())
            return Result.Fail<RegisterUserResponse>(new Error("Erro ao registrar usuário."));

        var errors = result.Errors.Select(e => new Error(e.Description));

        return Result.Fail<RegisterUserResponse>(errors);
    }


    public async Task<Result<UserLoginResponse>> Login(UserLoginRequest userLogin)
    {
        var user = await _userManager.FindByEmailAsync(userLogin.Email);

        if (user is null)
        {
            return Result.Fail<UserLoginResponse>(
                new Error("Usuário ou senha estão incorretos")
                    .WithCode(ProblemCode.UnauthorizedUser.ToString()));
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, userLogin.Password, true);
        if (result.Succeeded)
        {
            var response = await GenerateCredentials(user);
            return Result.Ok(response);
        }

        Error error;
        if (result.IsLockedOut)
            error = new Error("Essa conta está bloqueada");
        else if (result.IsNotAllowed)
            error = new Error("Essa conta não tem permissão para fazer login");
        else if (result.RequiresTwoFactor)
            error = new Error("É necessário confirmar o login no seu segundo fator de autenticação");
        else
            error = new Error("Usuário ou senha estão incorretos");

        error = error.WithCode(ProblemCode.UnauthorizedUser.ToString());

        return Result.Fail<UserLoginResponse>(error);
    }


    public async Task<Result<UserLoginResponse>> RefreshLogin(string refreshToken)
    {
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
                return Result.Fail<UserLoginResponse>(
                    new Error("Refresh token inválido")
                        .WithCode(ProblemCode.UnauthorizedUser.ToString()));
            }
        }
        catch (SecurityTokenExpiredException)
        {
            return Result.Fail<UserLoginResponse>(
                new Error("Refresh token expirado")
                    .WithCode(ProblemCode.UnauthorizedUser.ToString()));
        }
        catch
        {
            return Result.Fail<UserLoginResponse>(
                new Error("Refresh token inválido")
                    .WithCode(ProblemCode.UnauthorizedUser.ToString()));
        }

        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Fail<UserLoginResponse>(
                new Error("Usuário não encontrado")
                    .WithCode(ProblemCode.UnauthorizedUser.ToString()));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result.Fail<UserLoginResponse>(
                new Error("Usuário não encontrado")
                    .WithCode(ProblemCode.UnauthorizedUser.ToString()));
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Result.Fail<UserLoginResponse>(
                new Error("Essa conta está bloqueada")
                    .WithCode(ProblemCode.UnauthorizedUser.ToString()));
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return Result.Fail<UserLoginResponse>(
                new Error("Essa conta precisa confirmar seu e-mail antes de realizar o login")
                    .WithCode(ProblemCode.UnauthorizedUser.ToString()));
        }

        var loginResponse = await GenerateCredentials(user);
        return Result.Ok(loginResponse);
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