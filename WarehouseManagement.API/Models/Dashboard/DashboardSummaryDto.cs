namespace WarehouseManagement.API.Models.Dashboard;

public class DashboardSummaryDto
{
    public int TotalProducts { get; set; }
    public int TotalWarehouses { get; set; }
    public int LowStockProducts { get; set; }
    public int PendingStocktakes { get; set; }
    public decimal TotalInventoryQuantity { get; set; }
    public decimal IncomingThisMonth { get; set; }
    public decimal OutgoingThisMonth { get; set; }
    public decimal NetMovementThisMonth { get; set; }
}

public class LowStockProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public decimal AvailableQuantity { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal Shortfall { get; set; }
}
