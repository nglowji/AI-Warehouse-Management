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

        var totalProducts = await _dbContext.Products.CountAsync(p => !p.IsDeleted);
        var totalWarehouses = await _dbContext.Warehouses.CountAsync(w => !w.IsDeleted);
        var lowStockProducts = await _dbContext.Inventories
            .Include(i => i.Product)
            .CountAsync(i => !i.IsDeleted && !i.Product!.IsDeleted && i.AvailableQuantity < i.Product.MinimumStock);
        var pendingStocktakes = await _dbContext.Stocktakes
            .CountAsync(s => !s.IsDeleted && s.Status != "CONFIRMED" && s.Status != "CLOSED");

        var totalInventoryQuantity = await _dbContext.Inventories
            .Where(i => !i.IsDeleted)
            .SumAsync(i => (decimal?)i.AvailableQuantity) ?? 0m;

        var incomingThisMonth = await _dbContext.GoodsReceiptDetails
            .Include(d => d.GoodsReceipt)
            .Where(d => !d.IsDeleted && !d.GoodsReceipt!.IsDeleted && d.GoodsReceipt.ConfirmedAt >= monthStart)
            .SumAsync(d => (decimal?)d.Quantity) ?? 0m;

        var outgoingThisMonth = await _dbContext.GoodsIssueDetails
            .Include(d => d.GoodsIssue)
            .Where(d => !d.IsDeleted && !d.GoodsIssue!.IsDeleted && d.GoodsIssue.ConfirmedAt >= monthStart)
            .SumAsync(d => (decimal?)d.Quantity) ?? 0m;

        return new DashboardSummaryDto
        {
            TotalProducts = totalProducts,
            TotalWarehouses = totalWarehouses,
            LowStockProducts = lowStockProducts,
            PendingStocktakes = pendingStocktakes,
            TotalInventoryQuantity = totalInventoryQuantity,
            IncomingThisMonth = incomingThisMonth,
            OutgoingThisMonth = outgoingThisMonth,
            NetMovementThisMonth = incomingThisMonth - outgoingThisMonth
        };
    }

    public async Task<List<LowStockProductDto>> GetLowStockProductsAsync()
    {
        var items = await _dbContext.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.Location)
            .Where(i => !i.IsDeleted && !i.Product!.IsDeleted && i.AvailableQuantity < i.Product.MinimumStock)
            .OrderBy(i => i.AvailableQuantity)
            .Take(20)
            .Select(i => new LowStockProductDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product!.Name,
                Sku = i.Product.Sku,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse!.Name,
                LocationId = i.LocationId,
                LocationName = i.Location!.Name,
                AvailableQuantity = i.AvailableQuantity,
                MinimumStock = i.Product.MinimumStock,
                Shortfall = i.Product.MinimumStock - i.AvailableQuantity
            })
            .ToListAsync();

        return items;
    }
}
