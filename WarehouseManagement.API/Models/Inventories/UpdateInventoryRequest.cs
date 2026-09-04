namespace WarehouseManagement.API.Models.Inventories;

public class UpdateInventoryRequest
{
    public Guid? ProductId { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? LocationId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? ReservedQuantity { get; set; }
}
