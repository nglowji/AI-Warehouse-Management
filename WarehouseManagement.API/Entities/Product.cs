namespace WarehouseManagement.API.Entities;

public class Product : BaseEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid SupplierId { get; set; }
    public Guid UnitId { get; set; }
    public string? Barcode { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
    public Category? Category { get; set; }
    public Supplier? Supplier { get; set; }
    public Unit? Unit { get; set; }
}
