namespace WarehouseManagement.API.Models.GoodsIssues;

public class UpdateGoodsIssueRequest
{
    public string? Code { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? Status { get; set; }
    public Guid? ConfirmedByUserId { get; set; }
    public List<UpdateGoodsIssueDetailRequest>? Details { get; set; }
}

public class UpdateGoodsIssueDetailRequest
{
    public Guid? Id { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public decimal? Quantity { get; set; }
}
