namespace WarehouseManagement.API.Models.GoodsIssues;

public class CreateGoodsIssueRequest
{
    public string Code { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string Status { get; set; } = "DRAFT";
    public Guid? CreatedByUserId { get; set; }
    public List<CreateGoodsIssueDetailRequest> Details { get; set; } = new();
}

public class CreateGoodsIssueDetailRequest
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
}
