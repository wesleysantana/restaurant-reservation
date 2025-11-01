using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.Table;
public record TableOutput
{
    public string Name { get; set; }
    public StatusTable Status { get; set; }

    public TableOutput(string name, StatusTable status)
    {
        Name = name;
        Status = status;
    }
}