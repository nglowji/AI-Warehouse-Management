using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Entities;
using WarehouseManagement.API.Models.GoodsReceipts;

namespace WarehouseManagement.API.Services;

public class GoodsReceiptService : IGoodsReceiptService
{
    private readonly WarehouseDbContext _dbContext;

    public GoodsReceiptService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GoodsReceiptDto>> GetAllAsync()
    {
        var receipts = await _dbContext.GoodsReceipts
            .Include(r => r.Supplier)
            .Include(r => r.Warehouse)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product)
            .Include(r => r.Details)
                .ThenInclude(d => d.Location)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return receipts.Select(MapToDto).ToList();
    }

    public async Task<GoodsReceiptDto?> GetByIdAsync(Guid id)
    {
        var receipt = await _dbContext.GoodsReceipts
            .Include(r => r.Supplier)
            .Include(r => r.Warehouse)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product)
            .Include(r => r.Details)
                .ThenInclude(d => d.Location)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        return receipt is null ? null : MapToDto(receipt);
    }

    public async Task<GoodsReceiptDto> CreateAsync(CreateGoodsReceiptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Receipt code is required.");

        if (request.SupplierId == Guid.Empty)
            throw new ArgumentException("Supplier is required.");

        if (request.WarehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse is required.");

        var supplierExists = await _dbContext.Suppliers.AnyAsync(s => s.Id == request.SupplierId && !s.IsDeleted);
        if (!supplierExists)
            throw new InvalidOperationException("Supplier not found.");

        var warehouseExists = await _dbContext.Warehouses.AnyAsync(w => w.Id == request.WarehouseId && !w.IsDeleted);
        if (!warehouseExists)
            throw new InvalidOperationException("Warehouse not found.");

        if (request.Details is null || !request.Details.Any())
            throw new InvalidOperationException("At least one receipt detail is required.");

        foreach (var detail in request.Details)
        {
            if (detail.ProductId == Guid.Empty)
                throw new ArgumentException("Product is required.");

            if (detail.LocationId == Guid.Empty)
                throw new ArgumentException("Location is required.");

            if (detail.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            var productExists = await _dbContext.Products.AnyAsync(p => p.Id == detail.ProductId && !p.IsDeleted);
            if (!productExists)
                throw new InvalidOperationException($"Product {detail.ProductId} not found.");

            var locationExists = await _dbContext.WarehouseLocations.AnyAsync(l =>
                l.Id == detail.LocationId && l.WarehouseId == request.WarehouseId && !l.IsDeleted);
            if (!locationExists)
                throw new InvalidOperationException($"Location {detail.LocationId} not found for warehouse.");
        }

        var receipt = new GoodsReceipt
        {
            Code = request.Code.Trim(),
            SupplierId = request.SupplierId,
            WarehouseId = request.WarehouseId,
            Status = request.Status,
            CreatedByUserId = request.CreatedByUserId,
            Details = new List<GoodsReceiptDetail>()
        };

        foreach (var detail in request.Details)
        {
            receipt.Details.Add(new GoodsReceiptDetail
            {
                ProductId = detail.ProductId,
                LocationId = detail.LocationId,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice
            });
        }

        _dbContext.GoodsReceipts.Add(receipt);
        await _dbContext.SaveChangesAsync();

        var created = await _dbContext.GoodsReceipts
            .Include(r => r.Supplier)
            .Include(r => r.Warehouse)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product)
            .Include(r => r.Details)
                .ThenInclude(d => d.Location)
            .FirstAsync(r => r.Id == receipt.Id);

        return MapToDto(created);
    }

    public async Task<GoodsReceiptDto?> UpdateAsync(Guid id, UpdateGoodsReceiptRequest request)
    {
        var receipt = await _dbContext.GoodsReceipts
            .Include(r => r.Supplier)
            .Include(r => r.Warehouse)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product)
            .Include(r => r.Details)
                .ThenInclude(d => d.Location)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (receipt is null)
            return null;

        if (request.Code is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new ArgumentException("Receipt code is required.");

            receipt.Code = request.Code.Trim();
        }

        if (request.SupplierId.HasValue)
        {
            if (request.SupplierId.Value == Guid.Empty)
                throw new ArgumentException("Supplier is required.");

            var supplierExists = await _dbContext.Suppliers.AnyAsync(s => s.Id == request.SupplierId.Value && !s.IsDeleted);
            if (!supplierExists)
                throw new InvalidOperationException("Supplier not found.");

            receipt.SupplierId = request.SupplierId.Value;
        }

        if (request.WarehouseId.HasValue)
        {
            if (request.WarehouseId.Value == Guid.Empty)
                throw new ArgumentException("Warehouse is required.");

            var warehouseExists = await _dbContext.Warehouses.AnyAsync(w => w.Id == request.WarehouseId.Value && !w.IsDeleted);
            if (!warehouseExists)
                throw new InvalidOperationException("Warehouse not found.");

            receipt.WarehouseId = request.WarehouseId.Value;
        }

        if (request.Status is not null)
            receipt.Status = request.Status.Trim();

        if (request.ConfirmedByUserId.HasValue)
            receipt.ConfirmedByUserId = request.ConfirmedByUserId.Value;

        if (request.Details is not null)
        {
            foreach (var detail in receipt.Details)
            {
                _dbContext.GoodsReceiptDetails.Remove(detail);
            }

            receipt.Details.Clear();

            foreach (var detail in request.Details)
            {
                if (detail.ProductId is null || detail.ProductId == Guid.Empty)
                    throw new ArgumentException("Product is required.");

                if (detail.LocationId is null || detail.LocationId == Guid.Empty)
                    throw new ArgumentException("Location is required.");

                if (detail.Quantity is null || detail.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero.");

                receipt.Details.Add(new GoodsReceiptDetail
                {
                    GoodsReceiptId = receipt.Id,
                    ProductId = detail.ProductId.Value,
                    LocationId = detail.LocationId.Value,
                    Quantity = detail.Quantity.Value,
                    UnitPrice = detail.UnitPrice ?? 0m
                });
            }
        }

        receipt.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(receipt);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var receipt = await _dbContext.GoodsReceipts.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (receipt is null)
            return false;

        receipt.IsDeleted = true;
        receipt.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static GoodsReceiptDto MapToDto(GoodsReceipt receipt)
    {
        return new GoodsReceiptDto
        {
            Id = receipt.Id,
            Code = receipt.Code,
            SupplierId = receipt.SupplierId,
            SupplierName = receipt.Supplier?.Name,
            WarehouseId = receipt.WarehouseId,
            WarehouseName = receipt.Warehouse?.Name,
            Status = receipt.Status,
            CreatedByUserId = receipt.CreatedByUserId,
            ConfirmedByUserId = receipt.ConfirmedByUserId,
            ConfirmedAt = receipt.ConfirmedAt,
            Details = receipt.Details
                .Where(d => !d.IsDeleted)
                .Select(d => new GoodsReceiptDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name,
                    ProductSku = d.Product?.Sku,
                    LocationId = d.LocationId,
                    LocationName = d.Location?.Name,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList()
        };
    }
}
