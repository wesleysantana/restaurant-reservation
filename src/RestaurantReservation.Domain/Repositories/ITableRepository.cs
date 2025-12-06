using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Domain.Repositories;

public interface ITableRepository
{
    Task<IReadOnlyList<Table>> GetAllAsync(CancellationToken cancellationToken);
    Task<Table?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Table table, CancellationToken cancellationToken);
    Task UpdateAsync(Table table, CancellationToken cancellationToken);
    Task DeleteAsync(Table table, CancellationToken cancellationToken);
}