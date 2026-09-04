using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Entities;
using WarehouseManagement.API.Models.Categories;

namespace WarehouseManagement.API.Services;

public class CategoryService : ICategoryService
{
    private readonly WarehouseDbContext _dbContext;

    public CategoryService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _dbContext.Categories
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        return category is null ? null : MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Category name is required.");

        var exists = await _dbContext.Categories.AnyAsync(c => c.Name == request.Name && !c.IsDeleted);
        if (exists)
            throw new InvalidOperationException("Category already exists.");

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            IsActive = request.IsActive
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (category is null)
            return null;

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Category name is required.");

        category.Name = request.Name.Trim();
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (category is null)
            return false;

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        };
    }
}
