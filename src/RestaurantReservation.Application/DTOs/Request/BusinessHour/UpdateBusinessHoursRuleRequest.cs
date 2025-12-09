using RestaurantReservation.Application.Utils;
using RestaurantReservation.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Request.BusinessHour;

public class UpdateBusinessHoursRuleRequest
{
    [Required(ErrorMessage = MessagesDataAnnotations.Required)]
    public Guid Id { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public DateOnly? SpecificDate { get; set; }
    public WeekDay? WeekDay { get; set; }

    public TimeOnly? Open { get; set; }
    public TimeOnly? Close { get; set; }

    public bool IsClosed { get; set; }
}