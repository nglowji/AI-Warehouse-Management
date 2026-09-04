using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Entities;
using WarehouseManagement.API.Models.Products;

namespace WarehouseManagement.API.Services;

public class ProductService : IProductService
{
    private readonly WarehouseDbContext _dbContext;

    public ProductService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Unit)
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Unit)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return product is null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Sku))
            throw new ArgumentException("SKU is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Product name is required.");

        if (request.CategoryId == Guid.Empty)
            throw new ArgumentException("Category is required.");

        if (request.SupplierId == Guid.Empty)
            throw new ArgumentException("Supplier is required.");

        if (request.UnitId == Guid.Empty)
            throw new ArgumentException("Unit is required.");

        var categoryExists = await _dbContext.Categories.AnyAsync(c => c.Id == request.CategoryId && !c.IsDeleted);
        if (!categoryExists)
            throw new InvalidOperationException("Category not found.");

        var supplierExists = await _dbContext.Suppliers.AnyAsync(s => s.Id == request.SupplierId && !s.IsDeleted);
        if (!supplierExists)
            throw new InvalidOperationException("Supplier not found.");

        var unitExists = await _dbContext.Units.AnyAsync(u => u.Id == request.UnitId && !u.IsDeleted);
        if (!unitExists)
            throw new InvalidOperationException("Unit not found.");

        var skuExists = await _dbContext.Products.AnyAsync(p => p.Sku == request.Sku.Trim() && !p.IsDeleted);
        if (skuExists)
            throw new InvalidOperationException("SKU already exists.");

        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            var barcodeExists = await _dbContext.Products.AnyAsync(p => p.Barcode == request.Barcode.Trim() && !p.IsDeleted);
            if (barcodeExists)
                throw new InvalidOperationException("Barcode already exists.");
        }

        var product = new Product
        {
            Sku = request.Sku.Trim(),
            Name = request.Name.Trim(),
            CategoryId = request.CategoryId,
            SupplierId = request.SupplierId,
            UnitId = request.UnitId,
            Barcode = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim(),
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            MinimumStock = request.MinimumStock,
            IsActive = request.IsActive
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var created = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Unit)
            .FirstAsync(p => p.Id == product.Id);

        return MapToDto(created);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Unit)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (product is null)
            return null;

        if (request.Sku is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Sku))
                throw new ArgumentException("SKU is required.");

            product.Sku = request.Sku.Trim();
        }

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Product name is required.");

            product.Name = request.Name.Trim();
        }

        if (request.CategoryId.HasValue)
        {
            if (request.CategoryId.Value == Guid.Empty)
                throw new ArgumentException("Category is required.");

            var categoryExists = await _dbContext.Categories.AnyAsync(c => c.Id == request.CategoryId.Value && !c.IsDeleted);
            if (!categoryExists)
                throw new InvalidOperationException("Category not found.");

            product.CategoryId = request.CategoryId.Value;
        }

        if (request.SupplierId.HasValue)
        {
            if (request.SupplierId.Value == Guid.Empty)
                throw new ArgumentException("Supplier is required.");

            var supplierExists = await _dbContext.Suppliers.AnyAsync(s => s.Id == request.SupplierId.Value && !s.IsDeleted);
            if (!supplierExists)
                throw new InvalidOperationException("Supplier not found.");

            product.SupplierId = request.SupplierId.Value;
        }

        if (request.UnitId.HasValue)
        {
            if (request.UnitId.Value == Guid.Empty)
                throw new ArgumentException("Unit is required.");

            var unitExists = await _dbContext.Units.AnyAsync(u => u.Id == request.UnitId.Value && !u.IsDeleted);
            if (!unitExists)
                throw new InvalidOperationException("Unit not found.");

            product.UnitId = request.UnitId.Value;
        }

        if (request.Barcode is not null)
            product.Barcode = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim();

        if (request.CostPrice.HasValue)
            product.CostPrice = request.CostPrice.Value;

        if (request.SellingPrice.HasValue)
            product.SellingPrice = request.SellingPrice.Value;

        if (request.MinimumStock.HasValue)
            product.MinimumStock = request.MinimumStock.Value;

        if (request.IsActive.HasValue)
            product.IsActive = request.IsActive.Value;

        product.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (product is null)
            return false;

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            SupplierId = product.SupplierId,
            SupplierName = product.Supplier?.Name,
            UnitId = product.UnitId,
            UnitName = product.Unit?.Name,
            Barcode = product.Barcode,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            MinimumStock = product.MinimumStock,
            IsActive = product.IsActive
        };
    }
}
