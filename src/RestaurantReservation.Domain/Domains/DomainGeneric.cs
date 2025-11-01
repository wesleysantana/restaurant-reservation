namespace RestaurantReservation.Domain.Domains;

public abstract class DomainGeneric
{
    public Guid Id { get; private set; } = Guid.NewGuid();
}