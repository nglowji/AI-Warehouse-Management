namespace WarehouseManagement.API.Entities;

public class Stocktake : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string Status { get; set; } = "DRAFT";
    public Guid? CreatedByUserId { get; set; }
    public Guid? ConfirmedByUserId { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<StocktakeDetail> Details { get; set; } = new List<StocktakeDetail>();
}

public class StocktakeDetail : BaseEntity
{
    public Guid StocktakeId { get; set; }
    public Stocktake? Stocktake { get; set; }
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal Difference => ActualQuantity - SystemQuantity;
    public Product? Product { get; set; }
    public WarehouseLocation? Location { get; set; }
}
