using RestaurantReservation.Application.DTOs.Request.User;
using RestaurantReservation.Application.DTOs.Response.User;

namespace RestaurantReservation.Application.Interfaces;

public interface IIdentityService
{
    Task<RegisterUserResponse> RegisterUser(RegisterUserRequest registerUser);

    Task<UserLoginResponse> Login(UserLoginRequest userLogin);

    Task<UserLoginResponse> RefreshLogin(string usuarioId);
}