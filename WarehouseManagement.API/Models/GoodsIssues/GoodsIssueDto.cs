namespace WarehouseManagement.API.Models.GoodsIssues;

public class GoodsIssueDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? CreatedByUserId { get; set; }
    public Guid? ConfirmedByUserId { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public List<GoodsIssueDetailDto> Details { get; set; } = new();
}

public class GoodsIssueDetailDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }
    public Guid LocationId { get; set; }
    public string? LocationName { get; set; }
    public decimal Quantity { get; set; }
}
