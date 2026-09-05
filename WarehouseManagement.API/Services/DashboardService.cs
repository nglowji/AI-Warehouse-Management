using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Models.Dashboard;

namespace WarehouseManagement.API.Services;

public class DashboardService : IDashboardService
{
    private readonly WarehouseDbContext _dbContext;

    public DashboardService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var lowStockQuery = _dbContext.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .Where(i => !i.IsDeleted && i.Product != null && !i.Product.IsDeleted && i.AvailableQuantity < i.Product.MinimumStock);

        var inventoryQuantities = await _dbContext.Inventories
            .AsNoTracking()
            .Where(i => !i.IsDeleted)
            .Select(i => i.Quantity - i.ReservedQuantity)
            .ToListAsync();

        var incomingQuantities = await _dbContext.GoodsReceiptDetails
            .AsNoTracking()
            .Include(d => d.GoodsReceipt)
            .Where(d => !d.IsDeleted && d.GoodsReceipt != null && !d.GoodsReceipt.IsDeleted && d.GoodsReceipt.Status == "CONFIRMED" && d.GoodsReceipt.ConfirmedAt >= monthStart)
            .Select(d => d.Quantity)
            .ToListAsync();

        var outgoingQuantities = await _dbContext.GoodsIssueDetails
            .AsNoTracking()
            .Include(d => d.GoodsIssue)
            .Where(d => !d.IsDeleted && d.GoodsIssue != null && !d.GoodsIssue.IsDeleted && d.GoodsIssue.Status == "CONFIRMED" && d.GoodsIssue.ConfirmedAt >= monthStart)
            .Select(d => d.Quantity)
            .ToListAsync();

        var incomingThisMonth = incomingQuantities.Sum();
        var outgoingThisMonth = outgoingQuantities.Sum();

        return new DashboardSummaryDto
        {
            TotalProducts = await _dbContext.Products.AsNoTracking().CountAsync(p => !p.IsDeleted),
            TotalWarehouses = await _dbContext.Warehouses.AsNoTracking().CountAsync(w => !w.IsDeleted),
            LowStockProducts = await lowStockQuery.CountAsync(),
            PendingStocktakes = await _dbContext.Stocktakes.AsNoTracking().CountAsync(s => !s.IsDeleted && s.Status != "CONFIRMED" && s.Status != "CLOSED"),
            TotalInventoryQuantity = inventoryQuantities.Sum(),
            IncomingThisMonth = incomingThisMonth,
            OutgoingThisMonth = outgoingThisMonth,
            NetMovementThisMonth = incomingThisMonth - outgoingThisMonth
        };
    }

    public async Task<List<LowStockProductDto>> GetLowStockProductsAsync()
    {
        var items = await _dbContext.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.Location)
            .Where(i => !i.IsDeleted && i.Product != null && i.Warehouse != null && i.Location != null && !i.Product.IsDeleted && i.AvailableQuantity < i.Product.MinimumStock)
            .Select(i => new LowStockProductDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product!.Name,
                Sku = i.Product.Sku,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse!.Name,
                LocationId = i.LocationId,
                LocationName = i.Location!.Name,
                AvailableQuantity = i.Quantity - i.ReservedQuantity,
                MinimumStock = i.Product.MinimumStock,
                Shortfall = i.Product.MinimumStock - (i.Quantity - i.ReservedQuantity)
            })
            .ToListAsync();

        return items
            .OrderByDescending(i => i.Shortfall)
            .ThenBy(i => i.ProductName)
            .Take(20)
            .ToList();
    }
}
