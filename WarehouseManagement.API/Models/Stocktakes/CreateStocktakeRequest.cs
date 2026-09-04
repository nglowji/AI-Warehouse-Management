namespace WarehouseManagement.API.Models.Stocktakes;

public class CreateStocktakeRequest
{
    public string Code { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string Status { get; set; } = "DRAFT";
    public Guid? CreatedByUserId { get; set; }
    public List<CreateStocktakeDetailRequest> Details { get; set; } = new();
}

public class CreateStocktakeDetailRequest
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
}
