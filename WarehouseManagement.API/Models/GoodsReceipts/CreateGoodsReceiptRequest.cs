namespace WarehouseManagement.API.Models.GoodsReceipts;

public class CreateGoodsReceiptRequest
{
    public string Code { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public string Status { get; set; } = "DRAFT";
    public Guid? CreatedByUserId { get; set; }
    public List<CreateGoodsReceiptDetailRequest> Details { get; set; } = new();
}

public class CreateGoodsReceiptDetailRequest
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
