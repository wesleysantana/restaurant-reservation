namespace RestaurantReservation.Domain.Domains;

public abstract class DomainBase
{
    public Guid Id { get; private set; } = Guid.NewGuid();
}