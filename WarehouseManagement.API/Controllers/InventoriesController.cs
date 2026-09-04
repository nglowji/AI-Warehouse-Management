using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.API.Models.Inventories;
using WarehouseManagement.API.Services;

namespace WarehouseManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoriesController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoriesController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var inventories = await _inventoryService.GetAllAsync();
        return Ok(inventories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var inventory = await _inventoryService.GetByIdAsync(id);
        return inventory is null ? NotFound() : Ok(inventory);
    }

    [HttpGet("warehouse/{warehouseId:guid}")]
    public async Task<IActionResult> GetByWarehouse(Guid warehouseId)
    {
        var inventories = await _inventoryService.GetByWarehouseAsync(warehouseId);
        return Ok(inventories);
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(Guid productId)
    {
        var inventories = await _inventoryService.GetByProductAsync(productId);
        return Ok(inventories);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryRequest request)
    {
        try
        {
            var created = await _inventoryService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInventoryRequest request)
    {
        try
        {
            var updated = await _inventoryService.UpdateAsync(id, request);
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
        var deleted = await _inventoryService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
