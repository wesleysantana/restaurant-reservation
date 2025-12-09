using FluentResults;
using NodaTime;
using RestaurantReservation.Application.DTOs.Request.BusinessHour;
using RestaurantReservation.Application.DTOs.Response.BusinessHourRule;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.Repositories;

namespace RestaurantReservation.Application.Services;

public class BusinessHoursAppService : IBusinessHoursAppService
{
    private readonly IBusinessHoursRuleRepository _repository;
    private readonly DateTimeZone _timeZone;

    public BusinessHoursAppService(IBusinessHoursRuleRepository repository, DateTimeZone timeZone)
    {
        _repository = repository;
        _timeZone = timeZone;
    }

    public async Task<Result<IReadOnlyCollection<BusinessHoursRuleResponse>>> GetAllAsync(CancellationToken ct)
    {
        var rules = await _repository.GetAllAsync(ct);

        var dtos = rules
            .Select(MapToResponse)
            .ToList()
            .AsReadOnly();

        return Result.Ok<IReadOnlyCollection<BusinessHoursRuleResponse>>(dtos);
    }

    public async Task<Result<BusinessHoursRuleResponse>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var rule = await _repository.GetByIdAsync(id, ct);
        if (rule is null)
            return Result.Fail<BusinessHoursRuleResponse>("Rule not found.");

        return Result.Ok(MapToResponse(rule));
    }

    public async Task<Result<BusinessHoursRuleResponse>> CreateAsync(
        CreateBusinessHoursRuleRequest request,
        CancellationToken ct)
    {
        // Pelo menos SpecificDate OU WeekDay precisa existir
        if (!request.SpecificDate.HasValue && !request.WeekDay.HasValue)
            return Result.Fail<BusinessHoursRuleResponse>("Either SpecificDate or WeekDay must be provided.");

        BusinessHoursRule rule;

        if (request.SpecificDate.HasValue && request.IsClosed)
        {
            // Feriado / data específica fechada: usamos o factory que já seta Start/End = SpecificDate
            rule = BusinessHoursRule.CreateClosedDate(request.SpecificDate.Value);
        }
        else
        {
            // Para regras que usam Start/End (sejam específicas ou semanais) validamos o intervalo
            if (request.EndDate < request.StartDate)
                return Result.Fail<BusinessHoursRuleResponse>("EndDate must be >= StartDate.");

            // Regra genérica (semanal ou específica com horário)
            rule = new BusinessHoursRule(
                request.StartDate,
                request.EndDate,
                request.SpecificDate,
                request.WeekDay,
                request.Open,
                request.Close,
                request.IsClosed);
        }

        await _repository.AddAsync(rule, ct);

        return Result.Ok(MapToResponse(rule));
    }

    public async Task<Result> UpdateAsync(UpdateBusinessHoursRuleRequest request, CancellationToken ct)
    {
        var rule = await _repository.GetByIdAsync(request.Id, ct);
        if (rule is null)
            return Result.Fail("Rule not found.");

        // Mesmo critério: pelo menos SpecificDate ou WeekDay
        if (!request.SpecificDate.HasValue && !request.WeekDay.HasValue)
            return Result.Fail("Either SpecificDate or WeekDay must be provided.");

        // Para update, regra fechada específica ainda é tratada pela própria entidade via Validate()
        // aqui checamos só o intervalo quando fizer sentido
        if (request.SpecificDate is null && request.EndDate < request.StartDate)
            return Result.Fail("EndDate must be >= StartDate.");

        rule.Update(
            request.StartDate,
            request.EndDate,
            request.SpecificDate,
            request.WeekDay,
            request.Open,
            request.Close,
            request.IsClosed);

        await _repository.UpdateAsync(rule, ct);

        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
    {
        var rule = await _repository.GetByIdAsync(id, ct);
        if (rule is null)
            return Result.Fail("Rule not found.");

        await _repository.DeleteAsync(rule, ct);

        return Result.Ok();
    }

    public async Task<bool> IsOpenAsync(Instant startsAt, Instant endsAt, CancellationToken ct)
    {
        // 1. Converte Instant -> LocalDateTime na timezone do restaurante
        var startLocal = startsAt.InZone(_timeZone).LocalDateTime;
        var endLocal = endsAt.InZone(_timeZone).LocalDateTime;

        var startDate = DateOnly.FromDateTime(startLocal.ToDateTimeUnspecified());
        var endDate = DateOnly.FromDateTime(endLocal.ToDateTimeUnspecified());

        var startTime = new TimeOnly(startLocal.TimeOfDay.Hour, startLocal.TimeOfDay.Minute, startLocal.TimeOfDay.Second);

        var endTime = new TimeOnly(endLocal.TimeOfDay.Hour, endLocal.TimeOfDay.Minute, endLocal.TimeOfDay.Second);
        //var startTime = TimeOnly.FromTimeSpan(startLocal.TimeOfDay.ToTimeSpan());
        //var endTime = TimeOnly.FromTimeSpan(endLocal.TimeOfDay.ToTimeSpan());

        if (startDate != endDate)
        {
            // pra simplificar: não aceitar reservas que atravessam dias
            return false;
        }

        var day = (WeekDay)startLocal.DayOfWeek;

        // 2. Busca regras para esse período e data
        var rules = await _repository.GetRulesForDateAsync(startDate, ct);

        // 3. Regra específica tem prioridade (feriado, exceção)
        var specificRule = rules.FirstOrDefault(r => r.SpecificDate == startDate);
        if (specificRule is not null)
        {
            if (specificRule.IsClosed) return false;
            return startTime >= specificRule.Open && endTime <= specificRule.Close;
        }

        // 4. Regra semanal
        var weeklyRule = rules.FirstOrDefault(r => r.WeekDay == day);
        if (weeklyRule is null) return false; // sem regra = fechado

        if (weeklyRule.IsClosed) return false;

        return startTime >= weeklyRule.Open && endTime <= weeklyRule.Close;
    }

    private static BusinessHoursRuleResponse MapToResponse(BusinessHoursRule rule)
    {
        return new BusinessHoursRuleResponse
        {
            Id = rule.Id,
            StartDate = rule.StartDate,
            EndDate = rule.EndDate,
            SpecificDate = rule.SpecificDate,
            WeekDay = rule.WeekDay,
            Open = rule.Open,
            Close = rule.Close,
            IsClosed = rule.IsClosed
        };
    }
}