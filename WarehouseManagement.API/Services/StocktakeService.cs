using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Entities;
using WarehouseManagement.API.Models.Stocktakes;

namespace WarehouseManagement.API.Services;

public class StocktakeService : IStocktakeService
{
    private readonly WarehouseDbContext _dbContext;

    public StocktakeService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<StocktakeDto>> GetAllAsync()
    {
        var stocktakes = await _dbContext.Stocktakes
            .Include(s => s.Warehouse)
            .Include(s => s.Details)
                .ThenInclude(d => d.Product)
            .Include(s => s.Details)
                .ThenInclude(d => d.Location)
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return stocktakes.Select(MapToDto).ToList();
    }

    public async Task<StocktakeDto?> GetByIdAsync(Guid id)
    {
        var stocktake = await _dbContext.Stocktakes
            .Include(s => s.Warehouse)
            .Include(s => s.Details)
                .ThenInclude(d => d.Product)
            .Include(s => s.Details)
                .ThenInclude(d => d.Location)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        return stocktake is null ? null : MapToDto(stocktake);
    }

    public async Task<StocktakeDto> CreateAsync(CreateStocktakeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Stocktake code is required.");

        if (request.WarehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse is required.");

        var warehouseExists = await _dbContext.Warehouses.AnyAsync(w => w.Id == request.WarehouseId && !w.IsDeleted);
        if (!warehouseExists)
            throw new InvalidOperationException("Warehouse not found.");

        if (request.Details is null || !request.Details.Any())
            throw new InvalidOperationException("At least one stocktake detail is required.");

        foreach (var detail in request.Details)
        {
            if (detail.ProductId == Guid.Empty)
                throw new ArgumentException("Product is required.");

            if (detail.LocationId == Guid.Empty)
                throw new ArgumentException("Location is required.");

            var productExists = await _dbContext.Products.AnyAsync(p => p.Id == detail.ProductId && !p.IsDeleted);
            if (!productExists)
                throw new InvalidOperationException($"Product {detail.ProductId} not found.");

            var locationExists = await _dbContext.WarehouseLocations.AnyAsync(l =>
                l.Id == detail.LocationId && l.WarehouseId == request.WarehouseId && !l.IsDeleted);
            if (!locationExists)
                throw new InvalidOperationException($"Location {detail.LocationId} not found for warehouse.");
        }

        var stocktake = new Stocktake
        {
            Code = request.Code.Trim(),
            WarehouseId = request.WarehouseId,
            Status = request.Status,
            CreatedByUserId = request.CreatedByUserId,
            Details = new List<StocktakeDetail>()
        };

        foreach (var detail in request.Details)
        {
            stocktake.Details.Add(new StocktakeDetail
            {
                ProductId = detail.ProductId,
                LocationId = detail.LocationId,
                SystemQuantity = detail.SystemQuantity,
                ActualQuantity = detail.ActualQuantity
            });
        }

        _dbContext.Stocktakes.Add(stocktake);
        await _dbContext.SaveChangesAsync();

        var created = await _dbContext.Stocktakes
            .Include(s => s.Warehouse)
            .Include(s => s.Details)
                .ThenInclude(d => d.Product)
            .Include(s => s.Details)
                .ThenInclude(d => d.Location)
            .FirstAsync(s => s.Id == stocktake.Id);

        return MapToDto(created);
    }

    public async Task<StocktakeDto?> UpdateAsync(Guid id, UpdateStocktakeRequest request)
    {
        var stocktake = await _dbContext.Stocktakes
            .Include(s => s.Warehouse)
            .Include(s => s.Details)
                .ThenInclude(d => d.Product)
            .Include(s => s.Details)
                .ThenInclude(d => d.Location)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (stocktake is null)
            return null;

        if (request.Code is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new ArgumentException("Stocktake code is required.");

            stocktake.Code = request.Code.Trim();
        }

        if (request.WarehouseId.HasValue)
        {
            if (request.WarehouseId.Value == Guid.Empty)
                throw new ArgumentException("Warehouse is required.");

            var warehouseExists = await _dbContext.Warehouses.AnyAsync(w => w.Id == request.WarehouseId.Value && !w.IsDeleted);
            if (!warehouseExists)
                throw new InvalidOperationException("Warehouse not found.");

            stocktake.WarehouseId = request.WarehouseId.Value;
        }

        if (request.Status is not null)
            stocktake.Status = request.Status.Trim();

        if (request.ConfirmedByUserId.HasValue)
            stocktake.ConfirmedByUserId = request.ConfirmedByUserId.Value;

        if (request.Details is not null)
        {
            foreach (var detail in stocktake.Details)
            {
                _dbContext.StocktakeDetails.Remove(detail);
            }

            stocktake.Details.Clear();

            foreach (var detail in request.Details)
            {
                if (detail.ProductId is null || detail.ProductId == Guid.Empty)
                    throw new ArgumentException("Product is required.");

                if (detail.LocationId is null || detail.LocationId == Guid.Empty)
                    throw new ArgumentException("Location is required.");

                stocktake.Details.Add(new StocktakeDetail
                {
                    StocktakeId = stocktake.Id,
                    ProductId = detail.ProductId.Value,
                    LocationId = detail.LocationId.Value,
                    SystemQuantity = detail.SystemQuantity ?? 0m,
                    ActualQuantity = detail.ActualQuantity ?? 0m
                });
            }
        }

        stocktake.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(stocktake);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var stocktake = await _dbContext.Stocktakes.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        if (stocktake is null)
            return false;

        stocktake.IsDeleted = true;
        stocktake.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static StocktakeDto MapToDto(Stocktake stocktake)
    {
        return new StocktakeDto
        {
            Id = stocktake.Id,
            Code = stocktake.Code,
            WarehouseId = stocktake.WarehouseId,
            WarehouseName = stocktake.Warehouse?.Name,
            Status = stocktake.Status,
            CreatedByUserId = stocktake.CreatedByUserId,
            ConfirmedByUserId = stocktake.ConfirmedByUserId,
            ConfirmedAt = stocktake.ConfirmedAt,
            Details = stocktake.Details
                .Where(d => !d.IsDeleted)
                .Select(d => new StocktakeDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name,
                    ProductSku = d.Product?.Sku,
                    LocationId = d.LocationId,
                    LocationName = d.Location?.Name,
                    SystemQuantity = d.SystemQuantity,
                    ActualQuantity = d.ActualQuantity,
                    Difference = d.ActualQuantity - d.SystemQuantity
                }).ToList()
        };
    }
}
