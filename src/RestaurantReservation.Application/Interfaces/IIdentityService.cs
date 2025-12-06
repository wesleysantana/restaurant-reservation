using FluentResults;
using RestaurantReservation.Application.DTOs.Request.User;
using RestaurantReservation.Application.DTOs.Response.User;

namespace RestaurantReservation.Application.Interfaces;

public interface IIdentityService
{
    Task<Result<RegisterUserResponse>> RegisterUser(RegisterUserRequest userRegistration);
    Task<Result<UserLoginResponse>> Login(UserLoginRequest userLogin);
    Task<Result<UserLoginResponse>> RefreshLogin(string refreshToken);
}