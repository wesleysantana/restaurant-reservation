using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.Response.Table;

public class TableResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public short Capacity { get; set; }
    public StatusTable Status { get; set; }
}