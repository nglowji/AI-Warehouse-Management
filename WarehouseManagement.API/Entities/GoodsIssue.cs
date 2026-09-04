namespace WarehouseManagement.API.Entities;

public class GoodsIssue : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string Status { get; set; } = "DRAFT";
    public Guid? CreatedByUserId { get; set; }
    public Guid? ConfirmedByUserId { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<GoodsIssueDetail> Details { get; set; } = new List<GoodsIssueDetail>();
}

public class GoodsIssueDetail : BaseEntity
{
    public Guid GoodsIssueId { get; set; }
    public GoodsIssue? GoodsIssue { get; set; }
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
    public Product? Product { get; set; }
    public WarehouseLocation? Location { get; set; }
}
