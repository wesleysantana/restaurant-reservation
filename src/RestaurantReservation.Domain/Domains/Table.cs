using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.ValueObjects;

namespace RestaurantReservation.Domain.Domains;

public class Table : DomainGeneric
{
    public Name Name { get; private set; }
    public Capacity Capacity { get; private set; }
    public StatusTable Status { get; private set; }

    public Table(Name name, Capacity capacity, StatusTable status)
    {
        Name = name;
        Capacity = capacity;
        Status = status;
    }

    public void Update(Name? name = null, Capacity? capacity = null, StatusTable? status = null)
    {
        Name = name ?? Name;
        Capacity = capacity ?? Capacity;
        Status = status ?? Status;
    }
}