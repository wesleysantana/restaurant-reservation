namespace RestaurantReservation.Application.DTOs.Response.User;

public class RegisterUserResponse
{
    public bool Success { get; private set; }
    public List<string> Errors { get; private set; }

    public RegisterUserResponse() => Errors = [];

    public RegisterUserResponse(bool success = true) : this() => Success = success;

    public void AddErrors(IEnumerable<string> errors) => Errors.AddRange(errors);
}