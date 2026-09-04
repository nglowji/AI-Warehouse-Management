using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Entities;
using WarehouseManagement.API.Models.Warehouses;

namespace WarehouseManagement.API.Services;

public class WarehouseService : IWarehouseService
{
    private readonly WarehouseDbContext _dbContext;

    public WarehouseService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WarehouseDto>> GetAllAsync()
    {
        var warehouses = await _dbContext.Warehouses
            .Where(w => !w.IsDeleted)
            .OrderBy(w => w.Name)
            .ToListAsync();

        return warehouses.Select(MapToDto).ToList();
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid id)
    {
        var warehouse = await _dbContext.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);

        return warehouse is null ? null : MapToDto(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Warehouse code is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Warehouse name is required.");

        var codeExists = await _dbContext.Warehouses.AnyAsync(w => w.Code == request.Code.Trim() && !w.IsDeleted);
        if (codeExists)
            throw new InvalidOperationException("Warehouse code already exists.");

        var warehouse = new Warehouse
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Address = request.Address,
            IsActive = request.IsActive
        };

        _dbContext.Warehouses.Add(warehouse);
        await _dbContext.SaveChangesAsync();

        return MapToDto(warehouse);
    }

    public async Task<WarehouseDto?> UpdateAsync(Guid id, UpdateWarehouseRequest request)
    {
        var warehouse = await _dbContext.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);

        if (warehouse is null)
            return null;

        if (request.Code is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new ArgumentException("Warehouse code is required.");

            warehouse.Code = request.Code.Trim();
        }

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Warehouse name is required.");

            warehouse.Name = request.Name.Trim();
        }

        if (request.Address is not null)
            warehouse.Address = request.Address;

        if (request.IsActive.HasValue)
            warehouse.IsActive = request.IsActive.Value;

        warehouse.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(warehouse);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var warehouse = await _dbContext.Warehouses.FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
        if (warehouse is null)
            return false;

        warehouse.IsDeleted = true;
        warehouse.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static WarehouseDto MapToDto(Warehouse warehouse)
    {
        return new WarehouseDto
        {
            Id = warehouse.Id,
            Code = warehouse.Code,
            Name = warehouse.Name,
            Address = warehouse.Address,
            IsActive = warehouse.IsActive
        };
    }
}
