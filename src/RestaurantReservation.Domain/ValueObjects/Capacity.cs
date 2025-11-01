using RestaurantReservation.Domain.Exceptions;

namespace RestaurantReservation.Domain.ValueObjects;

public record Capacity
{
    public int Value { get; private set; }

    public Capacity(int value)
    {
        Validate(value);
        Value = value;
    }

    private void Validate(int capacity)
    {
        if (capacity < 1)
            throw new DomainException("The capacity must have a value greater than zero");
    }
}