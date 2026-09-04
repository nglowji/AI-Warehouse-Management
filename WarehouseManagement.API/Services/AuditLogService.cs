using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Entities;
using WarehouseManagement.API.Models.AuditLogs;

namespace WarehouseManagement.API.Services;

public class AuditLogService : IAuditLogService
{
    private readonly WarehouseDbContext _dbContext;

    public AuditLogService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AuditLogDto>> GetAllAsync()
    {
        var logs = await _dbContext.AuditLogs
            .Include(a => a.User)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return logs.Select(MapToDto).ToList();
    }

    public async Task<AuditLogDto?> GetByIdAsync(Guid id)
    {
        var log = await _dbContext.AuditLogs
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        return log is null ? null : MapToDto(log);
    }

    public async Task<AuditLogDto> CreateAsync(CreateAuditLogRequest request)
    {
        if (request.UserId == Guid.Empty)
            throw new ArgumentException("User is required.");

        if (string.IsNullOrWhiteSpace(request.Action))
            throw new ArgumentException("Action is required.");

        if (string.IsNullOrWhiteSpace(request.EntityName))
            throw new ArgumentException("Entity name is required.");

        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.UserId && !u.IsDeleted);
        if (!userExists)
            throw new InvalidOperationException("User not found.");

        var auditLog = new AuditLog
        {
            UserId = request.UserId,
            Action = request.Action.Trim(),
            EntityName = request.EntityName.Trim(),
            EntityId = request.EntityId,
            OldValue = request.OldValue,
            NewValue = request.NewValue
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync();

        var created = await _dbContext.AuditLogs
            .Include(a => a.User)
            .FirstAsync(a => a.Id == auditLog.Id);

        return MapToDto(created);
    }

    public async Task<AuditLogDto?> UpdateAsync(Guid id, UpdateAuditLogRequest request)
    {
        var log = await _dbContext.AuditLogs
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (log is null)
            return null;

        if (request.UserId.HasValue)
        {
            if (request.UserId.Value == Guid.Empty)
                throw new ArgumentException("User is required.");

            var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.UserId.Value && !u.IsDeleted);
            if (!userExists)
                throw new InvalidOperationException("User not found.");

            log.UserId = request.UserId.Value;
        }

        if (request.Action is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Action))
                throw new ArgumentException("Action is required.");

            log.Action = request.Action.Trim();
        }

        if (request.EntityName is not null)
        {
            if (string.IsNullOrWhiteSpace(request.EntityName))
                throw new ArgumentException("Entity name is required.");

            log.EntityName = request.EntityName.Trim();
        }

        if (request.EntityId.HasValue)
            log.EntityId = request.EntityId.Value;

        if (request.OldValue is not null)
            log.OldValue = request.OldValue;

        if (request.NewValue is not null)
            log.NewValue = request.NewValue;

        log.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return MapToDto(log);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var log = await _dbContext.AuditLogs.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        if (log is null)
            return false;

        log.IsDeleted = true;
        log.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static AuditLogDto MapToDto(AuditLog log)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            UserId = log.UserId,
            UserName = log.User?.UserName ?? string.Empty,
            Action = log.Action,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            OldValue = log.OldValue,
            NewValue = log.NewValue,
            CreatedAt = log.CreatedAt
        };
    }
}
