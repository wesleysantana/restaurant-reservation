namespace RestaurantReservation.Application.DTOs.User;

public record UserOutput
{
    public string Name { get; private set; }
    public string Email { get; private set; }

    public UserOutput(string name, string email)
    {
        Name = name;
        Email = email;
    }
}