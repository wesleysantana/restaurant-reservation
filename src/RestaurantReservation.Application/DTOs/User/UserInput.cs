namespace RestaurantReservation.Application.DTOs.User;
public record UserInput
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }

    public UserInput(string name, string email, string password)
    {
        Name = name;
        Email = email;
        Password = password;
    }
}