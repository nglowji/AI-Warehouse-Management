using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.API.Models.WarehouseLocations;
using WarehouseManagement.API.Services;

namespace WarehouseManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarehouseLocationsController : ControllerBase
{
    private readonly IWarehouseLocationService _warehouseLocationService;

    public WarehouseLocationsController(IWarehouseLocationService warehouseLocationService)
    {
        _warehouseLocationService = warehouseLocationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var locations = await _warehouseLocationService.GetAllAsync();
        return Ok(locations);
    }

    [HttpGet("warehouse/{warehouseId:guid}")]
    public async Task<IActionResult> GetByWarehouse(Guid warehouseId)
    {
        var locations = await _warehouseLocationService.GetByWarehouseAsync(warehouseId);
        return Ok(locations);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var location = await _warehouseLocationService.GetByIdAsync(id);
        return location is null ? NotFound() : Ok(location);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseLocationRequest request)
    {
        try
        {
            var created = await _warehouseLocationService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseLocationRequest request)
    {
        try
        {
            var updated = await _warehouseLocationService.UpdateAsync(id, request);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _warehouseLocationService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
