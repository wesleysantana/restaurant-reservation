using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantReservation.Application.DTOs.Request.Table;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.WebApi.Extensions;
using RestaurantReservation.WebApi.Localization;

namespace RestaurantReservation.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TablesController : ControllerBase
{
    private readonly ITableAppService _tableAppService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public TablesController(ITableAppService tableAppService, IStringLocalizer<SharedResource> localizer)
    {
        _tableAppService = tableAppService;
        _localizer = localizer;
    }

    // GET /mesas
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _tableAppService.GetAllAsync(cancellationToken);

        return result.ToActionResult(this, _localizer, dtos => Ok(dtos));
    }

    // GET /mesas/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _tableAppService.GetByIdAsync(id, cancellationToken);

        return result.ToActionResult(this, _localizer, dto => Ok(dto));
    }

    // POST /mesas (apenas admin, como pede o desafio)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTableRequest request, CancellationToken ct)
    {
        var result = await _tableAppService.CreateAsync(request, ct);

        return result.ToActionResult(this, _localizer, dto => Created());
    }

    // PATCH /mesas/{id}
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTableRequest request, CancellationToken ct)
    {
        request.Id = id;

        var result = await _tableAppService.UpdateAsync(request, ct);

        return result.ToActionResult(this, _localizer, () => NoContent());
    }

    // DELETE /mesas/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _tableAppService.DeleteAsync(id, cancellationToken);

        return result.ToActionResult(this, _localizer, () => NoContent());
    }
}