using WarehouseManagement.API.Models.Inventories;

namespace WarehouseManagement.API.Services;

public interface IInventoryService
{
    Task<List<InventoryDto>> GetAllAsync();
    Task<InventoryDto?> GetByIdAsync(Guid id);
    Task<List<InventoryDto>> GetByWarehouseAsync(Guid warehouseId);
    Task<List<InventoryDto>> GetByProductAsync(Guid productId);
    Task<InventoryDto> CreateAsync(CreateInventoryRequest request);
    Task<InventoryDto?> UpdateAsync(Guid id, UpdateInventoryRequest request);
    Task<bool> DeleteAsync(Guid id);
}
