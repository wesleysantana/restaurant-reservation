using System.Text.Json.Serialization;

namespace RestaurantReservation.Application.DTOs.Response.User;

public class UserLoginResponse
{
    public bool Success => Errors.Count == 0;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string AccessToken { get; private set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string RefreshToken { get; private set; }

    public List<string> Errors { get; private set; }    

    public UserLoginResponse(string? accessToken = null, string? refreshToken = null)
    {
        AccessToken = accessToken ?? AccessToken!;
        RefreshToken = refreshToken ?? RefreshToken!;

        Errors = [];
    }

    public void AddError(string error) => Errors.Add(error);

    public void AddErrors(IEnumerable<string> errors) => Errors.AddRange(errors);
}