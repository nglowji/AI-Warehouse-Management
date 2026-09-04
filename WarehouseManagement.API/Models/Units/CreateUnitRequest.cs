namespace WarehouseManagement.API.Models.Units;

public class CreateUnitRequest
{
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
