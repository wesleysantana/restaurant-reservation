using RestaurantReservation.Domain.Exceptions;

namespace RestaurantReservation.Domain.ValueObjects;
public record Password
{
    public string Value { get; private set; }

    public Password(string value)
    {
        Validate(value);
        Value = value;
    }

    private void Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainException("The password field cannot be empty or null");

        if (password.Length < 6 || password.Length > 255)
            throw new DomainException("The password must be between 6 and 255 characters long");
    }
}