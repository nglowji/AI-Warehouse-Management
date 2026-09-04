namespace WarehouseManagement.API.Models.AuditLogs;

public class UpdateAuditLogRequest
{
    public Guid? UserId { get; set; }
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public Guid? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
