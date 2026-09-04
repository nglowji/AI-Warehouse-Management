namespace WarehouseManagement.API.Models.AI;

public class WarehouseAssistantRequest
{
    public string Question { get; set; } = string.Empty;
}

public class WarehouseAssistantResponse
{
    public string Answer { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
