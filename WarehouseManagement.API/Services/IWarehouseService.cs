using WarehouseManagement.API.Models.Warehouses;

namespace WarehouseManagement.API.Services;

public interface IWarehouseService
{
    Task<List<WarehouseDto>> GetAllAsync();
    Task<WarehouseDto?> GetByIdAsync(Guid id);
    Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request);
    Task<WarehouseDto?> UpdateAsync(Guid id, UpdateWarehouseRequest request);
    Task<bool> DeleteAsync(Guid id);
}
