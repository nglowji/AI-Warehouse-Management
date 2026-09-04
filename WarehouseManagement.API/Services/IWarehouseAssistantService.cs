using WarehouseManagement.API.Models.AI;

namespace WarehouseManagement.API.Services;

public interface IWarehouseAssistantService
{
    Task<WarehouseAssistantResponse> AskAsync(WarehouseAssistantRequest request);
}
