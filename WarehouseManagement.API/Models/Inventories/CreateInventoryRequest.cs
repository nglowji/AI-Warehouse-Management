namespace WarehouseManagement.API.Models.Inventories;

public class CreateInventoryRequest
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
}
