using WarehouseManagement.API.Models.AuditLogs;

namespace WarehouseManagement.API.Services;

public interface IAuditLogService
{
    Task<List<AuditLogDto>> GetAllAsync();
    Task<AuditLogDto?> GetByIdAsync(Guid id);
    Task<AuditLogDto> CreateAsync(CreateAuditLogRequest request);
    Task<AuditLogDto?> UpdateAsync(Guid id, UpdateAuditLogRequest request);
    Task<bool> DeleteAsync(Guid id);
}
