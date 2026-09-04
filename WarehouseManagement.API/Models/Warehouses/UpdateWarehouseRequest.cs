namespace WarehouseManagement.API.Models.Warehouses;

public class UpdateWarehouseRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public bool? IsActive { get; set; }
}
