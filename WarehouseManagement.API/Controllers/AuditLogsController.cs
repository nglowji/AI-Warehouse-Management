using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.API.Models.AuditLogs;
using WarehouseManagement.API.Services;

namespace WarehouseManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var logs = await _auditLogService.GetAllAsync();
        return Ok(logs);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var log = await _auditLogService.GetByIdAsync(id);
        return log is null ? NotFound() : Ok(log);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuditLogRequest request)
    {
        try
        {
            var created = await _auditLogService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAuditLogRequest request)
    {
        try
        {
            var updated = await _auditLogService.UpdateAsync(id, request);
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
        var deleted = await _auditLogService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
