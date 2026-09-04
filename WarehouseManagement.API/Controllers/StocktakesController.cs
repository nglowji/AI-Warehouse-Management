using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.API.Models.Stocktakes;
using WarehouseManagement.API.Services;

namespace WarehouseManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StocktakesController : ControllerBase
{
    private readonly IStocktakeService _stocktakeService;

    public StocktakesController(IStocktakeService stocktakeService)
    {
        _stocktakeService = stocktakeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var stocktakes = await _stocktakeService.GetAllAsync();
        return Ok(stocktakes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var stocktake = await _stocktakeService.GetByIdAsync(id);
        return stocktake is null ? NotFound() : Ok(stocktake);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStocktakeRequest request)
    {
        try
        {
            var created = await _stocktakeService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStocktakeRequest request)
    {
        try
        {
            var updated = await _stocktakeService.UpdateAsync(id, request);
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
        var deleted = await _stocktakeService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
