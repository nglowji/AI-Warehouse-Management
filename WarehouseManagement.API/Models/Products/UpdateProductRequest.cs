namespace WarehouseManagement.API.Models.Products;

public class UpdateProductRequest
{
    public string? Sku { get; set; }
    public string? Name { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? UnitId { get; set; }
    public string? Barcode { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal? MinimumStock { get; set; }
    public bool? IsActive { get; set; }
}
