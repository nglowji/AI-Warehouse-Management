using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Entities;
using WarehouseManagement.API.Models.Units;

namespace WarehouseManagement.API.Services;

public class UnitService : IUnitService
{
    private readonly WarehouseDbContext _dbContext;

    public UnitService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UnitDto>> GetAllAsync()
    {
        var units = await _dbContext.Units
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.Name)
            .ToListAsync();

        return units.Select(MapToDto).ToList();
    }

    public async Task<UnitDto?> GetByIdAsync(Guid id)
    {
        var unit = await _dbContext.Units
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        return unit is null ? null : MapToDto(unit);
    }

    public async Task<UnitDto> CreateAsync(CreateUnitRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Unit name is required.");

        if (string.IsNullOrWhiteSpace(request.ShortName))
            throw new ArgumentException("Short name is required.");

        var exists = await _dbContext.Units.AnyAsync(u =>
            (u.Name == request.Name.Trim() || u.ShortName == request.ShortName.Trim()) && !u.IsDeleted);

        if (exists)
            throw new InvalidOperationException("Unit already exists.");

        var unit = new Unit
        {
            Name = request.Name.Trim(),
            ShortName = request.ShortName.Trim(),
            IsActive = request.IsActive
        };

        _dbContext.Units.Add(unit);
        await _dbContext.SaveChangesAsync();

        return MapToDto(unit);
    }

    public async Task<UnitDto?> UpdateAsync(Guid id, UpdateUnitRequest request)
    {
        var unit = await _dbContext.Units
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (unit is null)
            return null;

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Unit name is required.");

            unit.Name = request.Name.Trim();
        }

        if (request.ShortName is not null)
        {
            if (string.IsNullOrWhiteSpace(request.ShortName))
                throw new ArgumentException("Short name is required.");

            unit.ShortName = request.ShortName.Trim();
        }

        if (request.IsActive.HasValue)
            unit.IsActive = request.IsActive.Value;

        unit.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(unit);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var unit = await _dbContext.Units.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (unit is null)
            return false;

        unit.IsDeleted = true;
        unit.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static UnitDto MapToDto(Unit unit)
    {
        return new UnitDto
        {
            Id = unit.Id,
            Name = unit.Name,
            ShortName = unit.ShortName,
            IsActive = unit.IsActive
        };
    }
}
