namespace WarehouseManagement.API.Entities;

public class Inventory : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity
    {
        get => Quantity - ReservedQuantity;
        set => _ = value;
    }

    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
    public WarehouseLocation? Location { get; set; }
}
