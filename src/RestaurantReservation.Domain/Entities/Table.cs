using RestaurantReservation.Domain.Entities.Base;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.ValueObjects;

namespace RestaurantReservation.Domain.Entities;

public class Table : EntityBase
{
    public Name Name { get; private set; }
    public Capacity Capacity { get; private set; }
    public StatusTable Status { get; private set; }

    // Constructor necessary for Ef Core
    private Table()
    {
        Name = new Name("");
        Capacity = new Capacity(0);
    }

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