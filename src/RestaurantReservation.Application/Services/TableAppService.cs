using FluentResults;
using RestaurantReservation.Application.DTOs.Request.Table;
using RestaurantReservation.Application.DTOs.Response.Table;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.Repositories;
using RestaurantReservation.Domain.ValueObjects;

namespace RestaurantReservation.Application.Services;

public class TableAppService : ITableAppService
{
    private readonly ITableRepository _tableRepository;

    public TableAppService(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task<Result<IReadOnlyCollection<TableResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var tables = await _tableRepository.GetAllAsync(cancellationToken);

        var dtos = tables
            .Select(t => new TableResponse
            {
                Id = t.Id,
                Name = t.Name.Value,
                Capacity = t.Capacity.Value,
                Status = t.Status
            })
            .ToList()
            .AsReadOnly();

        return Result.Ok<IReadOnlyCollection<TableResponse>>(dtos);
    }

    public async Task<Result<TableResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var table = await _tableRepository.GetByIdAsync(id, cancellationToken);

        if (table is null)
            return Result.Fail<TableResponse>("Table not found.");

        var dto = new TableResponse
        {
            Id = table.Id,
            Name = table.Name.Value,
            Capacity = table.Capacity.Value,
            Status = table.Status
        };

        return Result.Ok(dto);
    }

    public async Task<Result<TableResponse>> CreateAsync(CreateTableRequest request, CancellationToken cancellationToken)
    {
        // Aqui você pode colocar validações extras se quiser

        var name = new Name(request.Name);
        var capacity = new Capacity(request.Capacity);

        var table = new Table(name, capacity, StatusTable.Disponivel);

        await _tableRepository.AddAsync(table, cancellationToken);

        var dto = new TableResponse
        {
            Id = table.Id,
            Name = table.Name.Value,
            Capacity = table.Capacity.Value,
            Status = table.Status
        };

        return Result.Ok(dto);
    }

    public async Task<Result> UpdateAsync(UpdateTableRequest request, CancellationToken cancellationToken)
    {
        var table = await _tableRepository.GetByIdAsync(request.Id, cancellationToken);

        if (table is null)
            return Result.Fail("Table not found.");

        Name? name = request.Name is not null ? new Name(request.Name) : null;
        Capacity? capacity = request.Capacity.HasValue ? new Capacity(request.Capacity.Value) : null;

        table.Update(name, capacity, request.Status);

        await _tableRepository.UpdateAsync(table, cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var table = await _tableRepository.GetByIdAsync(id, cancellationToken);

        if (table is null)
            return Result.Fail("Table not found.");

        await _tableRepository.DeleteAsync(table, cancellationToken);

        return Result.Ok();
    }
}