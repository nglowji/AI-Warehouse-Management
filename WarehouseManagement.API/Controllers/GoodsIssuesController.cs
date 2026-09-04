using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.API.Models.GoodsIssues;
using WarehouseManagement.API.Services;

namespace WarehouseManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoodsIssuesController : ControllerBase
{
    private readonly IGoodsIssueService _goodsIssueService;

    public GoodsIssuesController(IGoodsIssueService goodsIssueService)
    {
        _goodsIssueService = goodsIssueService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var issues = await _goodsIssueService.GetAllAsync();
        return Ok(issues);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var issue = await _goodsIssueService.GetByIdAsync(id);
        return issue is null ? NotFound() : Ok(issue);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoodsIssueRequest request)
    {
        try
        {
            var created = await _goodsIssueService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGoodsIssueRequest request)
    {
        try
        {
            var updated = await _goodsIssueService.UpdateAsync(id, request);
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
        var deleted = await _goodsIssueService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
