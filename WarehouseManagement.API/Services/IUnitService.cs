using WarehouseManagement.API.Models.Units;

namespace WarehouseManagement.API.Services;

public interface IUnitService
{
    Task<List<UnitDto>> GetAllAsync();
    Task<UnitDto?> GetByIdAsync(Guid id);
    Task<UnitDto> CreateAsync(CreateUnitRequest request);
    Task<UnitDto?> UpdateAsync(Guid id, UpdateUnitRequest request);
    Task<bool> DeleteAsync(Guid id);
}
