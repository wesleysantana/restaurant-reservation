
using RestaurantReservation.Domain.Exceptions;

namespace RestaurantReservation.Domain.ValueObjects;

public record Name
{   
    public string Value { get; private set; }

    public Name(string value)
    {
        Validate(value);
        Value = value;
    }

    private void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("The name field cannot be empty or null");

        if (name.Length < 3 || name.Length > 255)
            throw new DomainException("The name must be between 3 and 255 characters long");
    }
}