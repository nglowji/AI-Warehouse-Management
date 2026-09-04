using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.API.Models.AI;
using WarehouseManagement.API.Services;

namespace WarehouseManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseAssistantController : ControllerBase
{
    private readonly IWarehouseAssistantService _assistantService;

    public WarehouseAssistantController(IWarehouseAssistantService assistantService)
    {
        _assistantService = assistantService;
    }

    [AllowAnonymous]
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] WarehouseAssistantRequest request)
    {
        var response = await _assistantService.AskAsync(request);
        return Ok(response);
    }
}
