using FluentResults;
using RestaurantReservation.Application.DTOs.Request.Table;
using RestaurantReservation.Application.DTOs.Response.Table;

namespace RestaurantReservation.Application.Interfaces;

public interface ITableAppService
{
    Task<Result<IReadOnlyCollection<TableResponse>>> GetAllAsync(CancellationToken cancellationToken);

    Task<Result<TableResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<TableResponse>> CreateAsync(CreateTableRequest request, CancellationToken cancellationToken);

    Task<Result> UpdateAsync(UpdateTableRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}