using FluentResults;
using NodaTime;
using RestaurantReservation.Application.DTOs.Request.BusinessHour;
using RestaurantReservation.Application.DTOs.Response.BusinessHourRule;

namespace RestaurantReservation.Application.Interfaces;

public interface IBusinessHoursAppService
{
    Task<Result<IReadOnlyCollection<BusinessHoursRuleResponse>>> GetAllAsync(CancellationToken ct);

    Task<Result<BusinessHoursRuleResponse>> GetByIdAsync(Guid id, CancellationToken ct);

    Task<Result<BusinessHoursRuleResponse>> CreateAsync(CreateBusinessHoursRuleRequest request, CancellationToken ct);

    Task<Result> UpdateAsync(UpdateBusinessHoursRuleRequest request, CancellationToken ct);

    Task<Result> DeleteAsync(Guid id, CancellationToken ct);

    Task<bool> IsOpenAsync(Instant startsAt, Instant endsAt, CancellationToken ct);
}