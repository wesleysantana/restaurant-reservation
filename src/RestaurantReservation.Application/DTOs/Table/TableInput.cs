using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.Table;

public record TableInput
{
    public string Name { get; private set; }
    public int Capacity { get; private set; }
    public StatusTable Status { get; private set; }

    public TableInput(string name, int capacity, StatusTable status)
    {
        Name = name;
        Capacity = capacity;
        Status = status;
    }
}