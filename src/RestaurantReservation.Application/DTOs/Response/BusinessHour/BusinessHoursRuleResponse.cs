using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.Response.BusinessHourRule;

public class BusinessHoursRuleResponse
{
    public Guid Id { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public DateOnly? SpecificDate { get; set; }
    public WeekDay? WeekDay { get; set; }

    public TimeOnly? Open { get; set; }
    public TimeOnly? Close { get; set; }

    public bool IsClosed { get; set; }
}