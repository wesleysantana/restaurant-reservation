namespace RestaurantReservation.Domain.Entities.Base;

public abstract class EntityBase : DomainBase
{
    public bool Active { get; protected set; } = true;
}