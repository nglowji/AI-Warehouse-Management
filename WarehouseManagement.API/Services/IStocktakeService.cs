using WarehouseManagement.API.Models.Stocktakes;

namespace WarehouseManagement.API.Services;

public interface IStocktakeService
{
    Task<List<StocktakeDto>> GetAllAsync();
    Task<StocktakeDto?> GetByIdAsync(Guid id);
    Task<StocktakeDto> CreateAsync(CreateStocktakeRequest request);
    Task<StocktakeDto?> UpdateAsync(Guid id, UpdateStocktakeRequest request);
    Task<bool> DeleteAsync(Guid id);
}
