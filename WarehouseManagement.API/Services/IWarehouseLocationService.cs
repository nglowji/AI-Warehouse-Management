using WarehouseManagement.API.Models.WarehouseLocations;

namespace WarehouseManagement.API.Services;

public interface IWarehouseLocationService
{
    Task<List<WarehouseLocationDto>> GetAllAsync();
    Task<WarehouseLocationDto?> GetByIdAsync(Guid id);
    Task<List<WarehouseLocationDto>> GetByWarehouseAsync(Guid warehouseId);
    Task<WarehouseLocationDto> CreateAsync(CreateWarehouseLocationRequest request);
    Task<WarehouseLocationDto?> UpdateAsync(Guid id, UpdateWarehouseLocationRequest request);
    Task<bool> DeleteAsync(Guid id);
}
