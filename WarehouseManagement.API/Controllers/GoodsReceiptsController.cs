using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.API.Models.GoodsReceipts;
using WarehouseManagement.API.Services;

namespace WarehouseManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoodsReceiptsController : ControllerBase
{
    private readonly IGoodsReceiptService _goodsReceiptService;

    public GoodsReceiptsController(IGoodsReceiptService goodsReceiptService)
    {
        _goodsReceiptService = goodsReceiptService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var receipts = await _goodsReceiptService.GetAllAsync();
        return Ok(receipts);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var receipt = await _goodsReceiptService.GetByIdAsync(id);
        return receipt is null ? NotFound() : Ok(receipt);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoodsReceiptRequest request)
    {
        try
        {
            var created = await _goodsReceiptService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGoodsReceiptRequest request)
    {
        try
        {
            var updated = await _goodsReceiptService.UpdateAsync(id, request);
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
        var deleted = await _goodsReceiptService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
