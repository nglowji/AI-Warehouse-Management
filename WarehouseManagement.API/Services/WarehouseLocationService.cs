using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Entities;
using WarehouseManagement.API.Models.WarehouseLocations;

namespace WarehouseManagement.API.Services;

public class WarehouseLocationService : IWarehouseLocationService
{
    private readonly WarehouseDbContext _dbContext;

    public WarehouseLocationService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WarehouseLocationDto>> GetAllAsync()
    {
        var locations = await _dbContext.WarehouseLocations
            .Include(l => l.Warehouse)
            .Where(l => !l.IsDeleted)
            .OrderBy(l => l.Name)
            .ToListAsync();

        return locations.Select(MapToDto).ToList();
    }

    public async Task<WarehouseLocationDto?> GetByIdAsync(Guid id)
    {
        var location = await _dbContext.WarehouseLocations
            .Include(l => l.Warehouse)
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

        return location is null ? null : MapToDto(location);
    }

    public async Task<List<WarehouseLocationDto>> GetByWarehouseAsync(Guid warehouseId)
    {
        var locations = await _dbContext.WarehouseLocations
            .Include(l => l.Warehouse)
            .Where(l => l.WarehouseId == warehouseId && !l.IsDeleted)
            .OrderBy(l => l.Code)
            .ToListAsync();

        return locations.Select(MapToDto).ToList();
    }

    public async Task<WarehouseLocationDto> CreateAsync(CreateWarehouseLocationRequest request)
    {
        if (request.WarehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse is required.");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Location code is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Location name is required.");

        var warehouseExists = await _dbContext.Warehouses.AnyAsync(w => w.Id == request.WarehouseId && !w.IsDeleted);
        if (!warehouseExists)
            throw new InvalidOperationException("Warehouse not found.");

        var exists = await _dbContext.WarehouseLocations.AnyAsync(l =>
            l.WarehouseId == request.WarehouseId && l.Code == request.Code.Trim() && !l.IsDeleted);

        if (exists)
            throw new InvalidOperationException("Location code already exists in this warehouse.");

        var location = new WarehouseLocation
        {
            WarehouseId = request.WarehouseId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            IsActive = request.IsActive
        };

        _dbContext.WarehouseLocations.Add(location);
        await _dbContext.SaveChangesAsync();

        var created = await _dbContext.WarehouseLocations
            .Include(l => l.Warehouse)
            .FirstAsync(l => l.Id == location.Id);

        return MapToDto(created);
    }

    public async Task<WarehouseLocationDto?> UpdateAsync(Guid id, UpdateWarehouseLocationRequest request)
    {
        var location = await _dbContext.WarehouseLocations
            .Include(l => l.Warehouse)
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

        if (location is null)
            return null;

        if (request.WarehouseId.HasValue)
        {
            if (request.WarehouseId.Value == Guid.Empty)
                throw new ArgumentException("Warehouse is required.");

            var warehouseExists = await _dbContext.Warehouses.AnyAsync(w => w.Id == request.WarehouseId.Value && !w.IsDeleted);
            if (!warehouseExists)
                throw new InvalidOperationException("Warehouse not found.");

            location.WarehouseId = request.WarehouseId.Value;
        }

        if (request.Code is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new ArgumentException("Location code is required.");

            location.Code = request.Code.Trim();
        }

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Location name is required.");

            location.Name = request.Name.Trim();
        }

        if (request.IsActive.HasValue)
            location.IsActive = request.IsActive.Value;

        location.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(location);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var location = await _dbContext.WarehouseLocations.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
        if (location is null)
            return false;

        location.IsDeleted = true;
        location.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static WarehouseLocationDto MapToDto(WarehouseLocation location)
    {
        return new WarehouseLocationDto
        {
            Id = location.Id,
            WarehouseId = location.WarehouseId,
            WarehouseName = location.Warehouse?.Name ?? string.Empty,
            Code = location.Code,
            Name = location.Name,
            IsActive = location.IsActive
        };
    }
}
