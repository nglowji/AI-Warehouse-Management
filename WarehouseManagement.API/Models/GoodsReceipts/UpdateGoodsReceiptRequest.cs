namespace WarehouseManagement.API.Models.GoodsReceipts;

public class UpdateGoodsReceiptRequest
{
    public string? Code { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? Status { get; set; }
    public Guid? ConfirmedByUserId { get; set; }
    public List<UpdateGoodsReceiptDetailRequest>? Details { get; set; }
}

public class UpdateGoodsReceiptDetailRequest
{
    public Guid? Id { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}
