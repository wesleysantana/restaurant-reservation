using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Repositories;
using RestaurantReservation.Infra.Context;

namespace RestaurantReservation.Infra.Repositories;

public class TableRepository : ITableRepository
{
    private readonly DataContext _context;

    public TableRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Table>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context
            .Tables
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Table?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context
            .Tables
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(Table table, CancellationToken cancellationToken)
    {
        await _context.Tables.AddAsync(table, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Table table, CancellationToken cancellationToken)
    {
        _context.Tables.Update(table);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Table table, CancellationToken cancellationToken)
    {
        _context.Tables.Remove(table);
        await _context.SaveChangesAsync(cancellationToken);
    }
}