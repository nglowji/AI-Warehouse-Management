using WarehouseManagement.API.Models.Dashboard;

namespace WarehouseManagement.API.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
    Task<List<LowStockProductDto>> GetLowStockProductsAsync();
}
