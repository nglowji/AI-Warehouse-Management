namespace WarehouseManagement.API.Entities;

public class InventoryTransaction : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid LocationId { get; set; }
    public string Type { get; set; } = string.Empty; // RECEIPT, ISSUE, STOCKTAKE
    public decimal Quantity { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceCode { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
    public WarehouseLocation? Location { get; set; }
}
