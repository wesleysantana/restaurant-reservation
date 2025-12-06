using RestaurantReservation.Application.DTOs.Response.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantReservation.Application.Interfaces;

public interface ITable
{
    Task<TableResponse> GetTableByIdAsync(Guid tableId, CancellationToken cancellationToken);
}
