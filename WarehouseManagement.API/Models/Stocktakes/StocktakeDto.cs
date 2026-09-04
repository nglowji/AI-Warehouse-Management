namespace WarehouseManagement.API.Models.Stocktakes;

public class StocktakeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? CreatedByUserId { get; set; }
    public Guid? ConfirmedByUserId { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public List<StocktakeDetailDto> Details { get; set; } = new();
}

public class StocktakeDetailDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }
    public Guid LocationId { get; set; }
    public string? LocationName { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal Difference { get; set; }
}
