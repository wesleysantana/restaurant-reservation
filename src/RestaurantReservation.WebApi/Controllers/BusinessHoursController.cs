using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantReservation.Application.DTOs.Request.BusinessHour;
using RestaurantReservation.Application.DTOs.Response.BusinessHourRule;
using RestaurantReservation.Application.Interfaces;
using RestaurantReservation.WebApi.Extensions;
using RestaurantReservation.WebApi.Localization;

namespace RestaurantReservation.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class BusinessHoursController : ControllerBase
{
    private readonly IBusinessHoursAppService _businessHoursAppService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public BusinessHoursController(
        IBusinessHoursAppService businessHoursAppService, IStringLocalizer<SharedResource> localizer)
    {
        _businessHoursAppService = businessHoursAppService;
        _localizer = localizer;
    }

    // GET /api/businesshours
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _businessHoursAppService.GetAllAsync(cancellationToken);

        return result.ToActionResult(this, _localizer, dtos => Ok(dtos));
    }

    // GET /api/businesshours/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BusinessHoursRuleResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _businessHoursAppService.GetByIdAsync(id, cancellationToken);

        return (ActionResult)result.ToActionResult(this, _localizer, dto => Ok(dto));
    }

    // POST /api/businesshours
    [HttpPost]
    public async Task<ActionResult<BusinessHoursRuleResponse>> Create(
        [FromBody] CreateBusinessHoursRuleRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var result = await _businessHoursAppService.CreateAsync(request, ct);

        return (ActionResult)result.ToActionResult(
            this,
            _localizer,
            dto => Created($"/api/businesshours/{dto.Id}", dto));
    }

    // PUT /api/businesshours/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, 
        [FromBody] UpdateBusinessHoursRuleRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        request.Id = id;

        var result = await _businessHoursAppService.UpdateAsync(request, ct);

        return (ActionResult)result.ToActionResult(this, _localizer, () => NoContent());
    }

    // DELETE /api/businesshours/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _businessHoursAppService.DeleteAsync(id, ct);

        return (ActionResult)result.ToActionResult(this, _localizer, () => NoContent());
    }
}