namespace WarehouseManagement.API.Models.Units;

public class UpdateUnitRequest
{
    public string? Name { get; set; }
    public string? ShortName { get; set; }
    public bool? IsActive { get; set; }
}
