using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Entities;
using WarehouseManagement.API.Models.Inventories;

namespace WarehouseManagement.API.Services;

public class InventoryService : IInventoryService
{
    private readonly WarehouseDbContext _dbContext;

    public InventoryService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<InventoryDto>> GetAllAsync()
    {
        var inventories = await _dbContext.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.Location)
            .Where(i => !i.IsDeleted)
            .OrderBy(i => i.Product!.Name)
            .ToListAsync();

        return inventories.Select(MapToDto).ToList();
    }

    public async Task<InventoryDto?> GetByIdAsync(Guid id)
    {
        var inventory = await _dbContext.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.Location)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

        return inventory is null ? null : MapToDto(inventory);
    }

    public async Task<List<InventoryDto>> GetByWarehouseAsync(Guid warehouseId)
    {
        var inventories = await _dbContext.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.Location)
            .Where(i => i.WarehouseId == warehouseId && !i.IsDeleted)
            .OrderBy(i => i.Product!.Name)
            .ToListAsync();

        return inventories.Select(MapToDto).ToList();
    }

    public async Task<List<InventoryDto>> GetByProductAsync(Guid productId)
    {
        var inventories = await _dbContext.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.Location)
            .Where(i => i.ProductId == productId && !i.IsDeleted)
            .OrderBy(i => i.Warehouse!.Name)
            .ToListAsync();

        return inventories.Select(MapToDto).ToList();
    }

    public async Task<InventoryDto> CreateAsync(CreateInventoryRequest request)
    {
        if (request.ProductId == Guid.Empty)
            throw new ArgumentException("Product is required.");

        if (request.WarehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse is required.");

        if (request.LocationId == Guid.Empty)
            throw new ArgumentException("Location is required.");

        if (request.Quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.");

        if (request.ReservedQuantity < 0)
            throw new ArgumentException("Reserved quantity cannot be negative.");

        var productExists = await _dbContext.Products.AnyAsync(p => p.Id == request.ProductId && !p.IsDeleted);
        if (!productExists)
            throw new InvalidOperationException("Product not found.");

        var warehouseExists = await _dbContext.Warehouses.AnyAsync(w => w.Id == request.WarehouseId && !w.IsDeleted);
        if (!warehouseExists)
            throw new InvalidOperationException("Warehouse not found.");

        var locationExists = await _dbContext.WarehouseLocations.AnyAsync(l =>
            l.Id == request.LocationId && l.WarehouseId == request.WarehouseId && !l.IsDeleted);
        if (!locationExists)
            throw new InvalidOperationException("Warehouse location not found in this warehouse.");

        var inventoryExists = await _dbContext.Inventories.AnyAsync(i =>
            i.ProductId == request.ProductId && i.WarehouseId == request.WarehouseId && i.LocationId == request.LocationId && !i.IsDeleted);

        if (inventoryExists)
            throw new InvalidOperationException("Inventory already exists for this product, warehouse and location.");

        if (request.ReservedQuantity > request.Quantity)
            throw new InvalidOperationException("Reserved quantity cannot exceed total quantity.");

        var inventory = new Inventory
        {
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            LocationId = request.LocationId,
            Quantity = request.Quantity,
            ReservedQuantity = request.ReservedQuantity
        };

        _dbContext.Inventories.Add(inventory);
        await _dbContext.SaveChangesAsync();

        var created = await _dbContext.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.Location)
            .FirstAsync(i => i.Id == inventory.Id);

        return MapToDto(created);
    }

    public async Task<InventoryDto?> UpdateAsync(Guid id, UpdateInventoryRequest request)
    {
        var inventory = await _dbContext.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.Location)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

        if (inventory is null)
            return null;

        if (request.ProductId.HasValue)
        {
            if (request.ProductId.Value == Guid.Empty)
                throw new ArgumentException("Product is required.");

            var productExists = await _dbContext.Products.AnyAsync(p => p.Id == request.ProductId.Value && !p.IsDeleted);
            if (!productExists)
                throw new InvalidOperationException("Product not found.");

            inventory.ProductId = request.ProductId.Value;
        }

        if (request.WarehouseId.HasValue)
        {
            if (request.WarehouseId.Value == Guid.Empty)
                throw new ArgumentException("Warehouse is required.");

            var warehouseExists = await _dbContext.Warehouses.AnyAsync(w => w.Id == request.WarehouseId.Value && !w.IsDeleted);
            if (!warehouseExists)
                throw new InvalidOperationException("Warehouse not found.");

            inventory.WarehouseId = request.WarehouseId.Value;
        }

        if (request.LocationId.HasValue)
        {
            if (request.LocationId.Value == Guid.Empty)
                throw new ArgumentException("Location is required.");

            var locationExists = await _dbContext.WarehouseLocations.AnyAsync(l =>
                l.Id == request.LocationId.Value && l.WarehouseId == inventory.WarehouseId && !l.IsDeleted);
            if (!locationExists)
                throw new InvalidOperationException("Warehouse location not found in this warehouse.");

            inventory.LocationId = request.LocationId.Value;
        }

        if (request.Quantity.HasValue)
        {
            if (request.Quantity.Value < 0)
                throw new ArgumentException("Quantity cannot be negative.");

            inventory.Quantity = request.Quantity.Value;
        }

        if (request.ReservedQuantity.HasValue)
        {
            if (request.ReservedQuantity.Value < 0)
                throw new ArgumentException("Reserved quantity cannot be negative.");

            if (request.ReservedQuantity.Value > inventory.Quantity)
                throw new InvalidOperationException("Reserved quantity cannot exceed total quantity.");

            inventory.ReservedQuantity = request.ReservedQuantity.Value;
        }

        inventory.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(inventory);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var inventory = await _dbContext.Inventories.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        if (inventory is null)
            return false;

        inventory.IsDeleted = true;
        inventory.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static InventoryDto MapToDto(Inventory inventory)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            ProductName = inventory.Product?.Name,
            ProductSku = inventory.Product?.Sku,
            WarehouseId = inventory.WarehouseId,
            WarehouseName = inventory.Warehouse?.Name,
            LocationId = inventory.LocationId,
            LocationName = inventory.Location?.Name,
            Quantity = inventory.Quantity,
            ReservedQuantity = inventory.ReservedQuantity,
            AvailableQuantity = inventory.Quantity - inventory.ReservedQuantity
        };
    }
}
