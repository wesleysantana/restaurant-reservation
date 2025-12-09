using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Repositories;
using RestaurantReservation.Infra.Context;

namespace RestaurantReservation.Infra.Repositories;

public class BusinessHoursRuleRepository : IBusinessHoursRuleRepository
{
    private readonly DataContext _context;
    private DbSet<BusinessHoursRule> Set => _context.Set<BusinessHoursRule>();

    public BusinessHoursRuleRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BusinessHoursRule>> GetAllAsync(CancellationToken cancellationToken)
        => await Set.ToListAsync(cancellationToken);

    public async Task<BusinessHoursRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await Set.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task AddAsync(BusinessHoursRule rule, CancellationToken cancellationToken)
    {
        await Set.AddAsync(rule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(BusinessHoursRule rule, CancellationToken cancellationToken)
    {
        Set.Update(rule);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(BusinessHoursRule rule, CancellationToken cancellationToken)
    {
        Set.Remove(rule);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BusinessHoursRule>> GetRulesForDateAsync(
        DateOnly date,
        CancellationToken cancellationToken)
    {
        return await Set
            .Where(r => r.StartDate <= date && r.EndDate >= date)
            .ToListAsync(cancellationToken);
    }
}
