using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Entities;
using WarehouseManagement.API.Models.Suppliers;

namespace WarehouseManagement.API.Services;

public class SupplierService : ISupplierService
{
    private readonly WarehouseDbContext _dbContext;

    public SupplierService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SupplierDto>> GetAllAsync()
    {
        var suppliers = await _dbContext.Suppliers
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return suppliers.Select(MapToDto).ToList();
    }

    public async Task<SupplierDto?> GetByIdAsync(Guid id)
    {
        var supplier = await _dbContext.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        return supplier is null ? null : MapToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Supplier name is required.");

        var exists = await _dbContext.Suppliers.AnyAsync(s => s.Name == request.Name.Trim() && !s.IsDeleted);
        if (exists)
            throw new InvalidOperationException("Supplier already exists.");

        var supplier = new Supplier
        {
            Name = request.Name.Trim(),
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            IsActive = request.IsActive
        };

        _dbContext.Suppliers.Add(supplier);
        await _dbContext.SaveChangesAsync();

        return MapToDto(supplier);
    }

    public async Task<SupplierDto?> UpdateAsync(Guid id, UpdateSupplierRequest request)
    {
        var supplier = await _dbContext.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (supplier is null)
            return null;

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Supplier name is required.");

            supplier.Name = request.Name.Trim();
        }

        if (request.Phone is not null)
            supplier.Phone = request.Phone;

        if (request.Email is not null)
            supplier.Email = request.Email;

        if (request.Address is not null)
            supplier.Address = request.Address;

        if (request.IsActive.HasValue)
            supplier.IsActive = request.IsActive.Value;

        supplier.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(supplier);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        if (supplier is null)
            return false;

        supplier.IsDeleted = true;
        supplier.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static SupplierDto MapToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            IsActive = supplier.IsActive
        };
    }
}
