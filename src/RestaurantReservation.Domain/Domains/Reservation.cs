using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Domain.Domains;

public class Reservation : DomainGeneric
{
    public Guid UserId { get; private set; }
    public Guid TableId { get; private set; }
    public DateTime Period { get; private set; }
    public StatusReservation Status { get; private set; }

    public Reservation(Guid userId, Guid tableId, DateTime period, StatusReservation status)
    {
        UserId = userId;
        TableId = tableId;
        Period = period;
        Status = status;
    }
}