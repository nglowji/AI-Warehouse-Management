using WarehouseManagement.API.Models.GoodsIssues;

namespace WarehouseManagement.API.Services;

public interface IGoodsIssueService
{
    Task<List<GoodsIssueDto>> GetAllAsync();
    Task<GoodsIssueDto?> GetByIdAsync(Guid id);
    Task<GoodsIssueDto> CreateAsync(CreateGoodsIssueRequest request);
    Task<GoodsIssueDto?> UpdateAsync(Guid id, UpdateGoodsIssueRequest request);
    Task<bool> DeleteAsync(Guid id);
}
