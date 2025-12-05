namespace RestaurantReservation.Application.DTOs.Response.Table;

public class Table
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public short Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
}