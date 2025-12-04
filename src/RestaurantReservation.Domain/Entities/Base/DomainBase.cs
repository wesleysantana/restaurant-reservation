namespace RestaurantReservation.Domain.Entities.Base;

public abstract class DomainBase
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}