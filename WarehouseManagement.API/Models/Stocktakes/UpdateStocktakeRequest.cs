namespace WarehouseManagement.API.Models.Stocktakes;

public class UpdateStocktakeRequest
{
    public string? Code { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? Status { get; set; }
    public Guid? ConfirmedByUserId { get; set; }
    public List<UpdateStocktakeDetailRequest>? Details { get; set; }
}

public class UpdateStocktakeDetailRequest
{
    public Guid? Id { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public decimal? SystemQuantity { get; set; }
    public decimal? ActualQuantity { get; set; }
}
