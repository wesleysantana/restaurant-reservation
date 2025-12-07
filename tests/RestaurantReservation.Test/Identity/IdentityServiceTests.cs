using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using RestaurantReservation.Application.DTOs.Request.User;
using RestaurantReservation.Application.DTOs.Response.User;
using RestaurantReservation.Application.Utils;
using RestaurantReservation.Identity.Configurations;
using RestaurantReservation.Identity.Services;
using Xunit;

namespace RestaurantReservation.UnitTests.Identity
{
    public class IdentityServiceTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly JwtOptions _jwtOptionsValue;

        public IdentityServiceTests()
        {
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();

            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var identityOptions = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<IdentityUser>>();

            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _userManagerMock.Object,
                contextAccessor.Object,
                userPrincipalFactory.Object,
                identityOptions.Object,
                logger.Object,
                schemes.Object,
                confirmation.Object);

            // JwtOptions compatível com AuthenticationSetup
            var keyBytes = Encoding.ASCII.GetBytes("test-secret-key-123456789012345678901234567890123456789_ZbZ8RvQZqoJqg");
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

            _jwtOptionsValue = new JwtOptions
            {
                Issuer = "test-issuer",
                Audience = "test-audience",
                SigningCredentials = signingCredentials,
                AccessTokenExpirationInMinutes = 30,
                RefreshTokenExpirationInMinutes = 60
            };

            _jwtOptions = Options.Create(_jwtOptionsValue);
        }

        private IdentityService CreateService()
        {
            return new IdentityService(
                _signInManagerMock.Object,
                _userManagerMock.Object,
                _jwtOptions);
        }

        private string CreateJwt(string userId, string typ, DateTime? expires = null)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.Email, "user@test.com"),
                new("typ", typ)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtOptionsValue.Issuer,
                audience: _jwtOptionsValue.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow.AddMinutes(-1),
                expires: expires ?? DateTime.UtcNow.AddMinutes(30),
                signingCredentials: _jwtOptionsValue.SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ---------------------------
        // REGISTER
        // ---------------------------

        [Fact]
        public async Task RegisterUser_ShouldReturnSuccess_WhenIdentityUserIsCreated()
        {
            var request = new RegisterUserRequest
            {
                Email = "user@test.com",
                Password = "StrongP@ssw0rd",
                PasswordConfirmation = "StrongP@ssw0rd"
            };

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.SetLockoutEnabledAsync(It.IsAny<IdentityUser>(), false))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateService();

            Result<RegisterUserResponse> result = await service.RegisterUser(request);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Success);
            Assert.Empty(result.Value.Errors);
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnFailure_WhenIdentityReturnsErrors()
        {
            var request = new RegisterUserRequest
            {
                Email = "user@test.com",
                Password = "weak",
                PasswordConfirmation = "weak"
            };

            var identityError = new IdentityError { Description = "Invalid password" };

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            var service = CreateService();

            var result = await service.RegisterUser(request);

            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message == "Invalid password");
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnGenericFailure_WhenIdentityHasNoErrors()
        {
            var request = new RegisterUserRequest
            {
                Email = "user@test.com",
                Password = "weak",
                PasswordConfirmation = "weak"
            };

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Failed()); // sem erros

            var service = CreateService();

            var result = await service.RegisterUser(request);

            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Erro ao registrar usuário.", error.Message);
        }

        // ---------------------------
        // LOGIN
        // ---------------------------

        [Fact]
        public async Task Login_ShouldReturnFailure_WhenUserNotFound()
        {
            var request = new UserLoginRequest
            {
                Email = "notfound@test.com",
                Password = "whatever"
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((IdentityUser?)null);

            var service = CreateService();

            var result = await service.Login(request);

            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Usuário ou senha estão incorretos", error.Message);
            Assert.Equal(ProblemCode.UnauthorizedUser.ToString(), error.Metadata["Code"]);
        }

        [Fact]
        public async Task Login_ShouldReturnFailure_WhenPasswordIsInvalid()
        {
            var request = new UserLoginRequest
            {
                Email = "user@test.com",
                Password = "wrong"
            };

            var user = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                UserName = request.Email
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
                .ReturnsAsync(SignInResult.Failed);

            var service = CreateService();

            var result = await service.Login(request);

            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Usuário ou senha estão incorretos", error.Message);
            Assert.Equal(ProblemCode.UnauthorizedUser.ToString(), error.Metadata["Code"]);
        }

        [Fact]
        public async Task Login_ShouldReturnFailure_WhenUserIsLockedOut()
        {
            var request = new UserLoginRequest
            {
                Email = "user@test.com",
                Password = "whatever"
            };

            var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = request.Email };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            var signInResult = SignInResult.LockedOut;

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
                .ReturnsAsync(signInResult);

            var service = CreateService();

            var result = await service.Login(request);

            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Essa conta está bloqueada", error.Message);
            Assert.Equal(ProblemCode.UnauthorizedUser.ToString(), error.Metadata["Code"]);
        }

        [Fact]
        public async Task Login_ShouldReturnFailure_WhenUserIsNotAllowed()
        {
            var request = new UserLoginRequest
            {
                Email = "user@test.com",
                Password = "whatever"
            };

            var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = request.Email };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            var signInResult = SignInResult.NotAllowed;

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
                .ReturnsAsync(signInResult);

            var service = CreateService();

            var result = await service.Login(request);

            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Essa conta não tem permissão para fazer login", error.Message);
            Assert.Equal(ProblemCode.UnauthorizedUser.ToString(), error.Metadata["Code"]);
        }

        [Fact]
        public async Task Login_ShouldReturnFailure_WhenRequiresTwoFactor()
        {
            var request = new UserLoginRequest
            {
                Email = "user@test.com",
                Password = "whatever"
            };

            var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = request.Email };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            var signInResult = SignInResult.TwoFactorRequired;

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
                .ReturnsAsync(signInResult);

            var service = CreateService();

            var result = await service.Login(request);

            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("É necessário confirmar o login no seu segundo fator de autenticação", error.Message);
            Assert.Equal(ProblemCode.UnauthorizedUser.ToString(), error.Metadata["Code"]);
        }

        [Fact]
        public async Task Login_ShouldReturnTokens_WhenCredentialsAreValid()
        {
            var request = new UserLoginRequest
            {
                Email = "user@test.com",
                Password = "StrongP@ssw0rd"
            };

            var user = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                UserName = request.Email
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
                .ReturnsAsync(SignInResult.Success);

            _userManagerMock
                .Setup(x => x.GetClaimsAsync(user))
                .ReturnsAsync(new List<Claim>());

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            var service = CreateService();

            var result = await service.Login(request);

            Assert.True(result.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));
        }

        // ---------------------------
        // REFRESH LOGIN
        // ---------------------------

        [Fact]
        public async Task RefreshLogin_ShouldFail_WhenTokenIsInvalid()
        {
            const string invalidToken = "this-is-not-a-valid-jwt";
            var service = CreateService();

            var result = await service.RefreshLogin(invalidToken);

            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Refresh token inválido", error.Message);
            Assert.Equal(ProblemCode.UnauthorizedUser.ToString(), error.Metadata["Code"]);
        }

        [Fact]
        public async Task RefreshLogin_ShouldFail_WhenTokenHasWrongType()
        {
            var userId = Guid.NewGuid().ToString();
            var accessToken = CreateJwt(userId, "access");
            var service = CreateService();

            var result = await service.RefreshLogin(accessToken);

            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Refresh token inválido", error.Message);
            Assert.Equal(ProblemCode.UnauthorizedUser.ToString(), error.Metadata["Code"]);
        }
        

        [Fact]
        public async Task RefreshLogin_ShouldFail_WhenUserIdClaimIsMissing()
        {
            // cria um token refresh sem sub/NameIdentifier
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Email, "user@test.com"),
                new("typ", "refresh")
            };

            var token = new JwtSecurityToken(
                issuer: _jwtOptionsValue.Issuer,
                audience: _jwtOptionsValue.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow.AddMinutes(-1),
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: _jwtOptionsValue.SigningCredentials);

            var refreshToken = new JwtSecurityTokenHandler().WriteToken(token);
            var service = CreateService();

            var result = await service.RefreshLogin(refreshToken);

            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Usuário não encontrado", error.Message);
            Assert.Equal(ProblemCode.UnauthorizedUser.ToString(), error.Metadata["Code"]);
        }

        [Fact]
        public async Task RefreshLogin_ShouldFail_WhenUserNotFound()
        {
            var userId = Guid.NewGuid().ToString();
            var refreshToken = CreateJwt(userId, "refresh");
            var service = CreateService();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((IdentityUser?)null);

            var result = await service.RefreshLogin(refreshToken);

            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Usuário não encontrado", error.Message);
            Assert.Equal(ProblemCode.UnauthorizedUser.ToString(), error.Metadata["Code"]);
        }

        [Fact]
        public async Task RefreshLogin_ShouldFail_WhenUserIsLockedOut()
        {
            var userId = Guid.NewGuid().ToString();
            var refreshToken = CreateJwt(userId, "refresh");
            var user = new IdentityUser { Id = userId };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.IsLockedOutAsync(user))
                .ReturnsAsync(true);

            var service = CreateService();

            var result = await service.RefreshLogin(refreshToken);

            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Essa conta está bloqueada", error.Message);
            Assert.Equal(ProblemCode.UnauthorizedUser.ToString(), error.Metadata["Code"]);
        }

        [Fact]
        public async Task RefreshLogin_ShouldFail_WhenEmailNotConfirmed()
        {
            var userId = Guid.NewGuid().ToString();
            var refreshToken = CreateJwt(userId, "refresh");
            var user = new IdentityUser { Id = userId, Email = "user@test.com" };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.IsLockedOutAsync(user))
                .ReturnsAsync(false);

            _userManagerMock
                .Setup(x => x.IsEmailConfirmedAsync(user))
                .ReturnsAsync(false);

            var service = CreateService();

            var result = await service.RefreshLogin(refreshToken);

            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Essa conta precisa confirmar seu e-mail antes de realizar o login", error.Message);
            Assert.Equal(ProblemCode.UnauthorizedUser.ToString(), error.Metadata["Code"]);
        }

        [Fact]
        public async Task RefreshLogin_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            var userId = Guid.NewGuid().ToString();
            var refreshToken = CreateJwt(userId, "refresh");
            var user = new IdentityUser
            {
                Id = userId,
                Email = "user@test.com",
                UserName = "user@test.com"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.IsLockedOutAsync(user))
                .ReturnsAsync(false);

            _userManagerMock
                .Setup(x => x.IsEmailConfirmedAsync(user))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.GetClaimsAsync(user))
                .ReturnsAsync(new List<Claim>());

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            var service = CreateService();

            var result = await service.RefreshLogin(refreshToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));
        }
    }
}
