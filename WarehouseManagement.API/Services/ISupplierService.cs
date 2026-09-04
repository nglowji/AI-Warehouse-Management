using WarehouseManagement.API.Models.Suppliers;

namespace WarehouseManagement.API.Services;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetAllAsync();
    Task<SupplierDto?> GetByIdAsync(Guid id);
    Task<SupplierDto> CreateAsync(CreateSupplierRequest request);
    Task<SupplierDto?> UpdateAsync(Guid id, UpdateSupplierRequest request);
    Task<bool> DeleteAsync(Guid id);
}
