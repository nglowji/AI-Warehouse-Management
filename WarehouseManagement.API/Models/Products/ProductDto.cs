namespace WarehouseManagement.API.Models.Products;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public Guid UnitId { get; set; }
    public string? UnitName { get; set; }
    public string? Barcode { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; }
}
