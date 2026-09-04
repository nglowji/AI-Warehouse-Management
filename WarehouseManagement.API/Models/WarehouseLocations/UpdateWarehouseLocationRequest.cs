namespace WarehouseManagement.API.Models.WarehouseLocations;

public class UpdateWarehouseLocationRequest
{
    public Guid? WarehouseId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}
