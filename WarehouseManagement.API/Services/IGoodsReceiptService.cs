using WarehouseManagement.API.Models.GoodsReceipts;

namespace WarehouseManagement.API.Services;

public interface IGoodsReceiptService
{
    Task<List<GoodsReceiptDto>> GetAllAsync();
    Task<GoodsReceiptDto?> GetByIdAsync(Guid id);
    Task<GoodsReceiptDto> CreateAsync(CreateGoodsReceiptRequest request);
    Task<GoodsReceiptDto?> UpdateAsync(Guid id, UpdateGoodsReceiptRequest request);
    Task<bool> DeleteAsync(Guid id);
}
