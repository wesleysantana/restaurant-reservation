using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Domain.Repositories;

public interface IBusinessHoursRuleRepository
{
    Task<IReadOnlyList<BusinessHoursRule>> GetRulesForDateAsync(DateOnly startDate, CancellationToken ct);
    Task<IReadOnlyList<BusinessHoursRule>> GetAllAsync(CancellationToken cancellationToken);
    Task<BusinessHoursRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(BusinessHoursRule rule, CancellationToken cancellationToken);
    Task UpdateAsync(BusinessHoursRule rule, CancellationToken cancellationToken);
    Task DeleteAsync(BusinessHoursRule rule, CancellationToken cancellationToken);
}