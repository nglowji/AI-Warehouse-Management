using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Models.AI;

namespace WarehouseManagement.API.Services;

public class WarehouseAssistantService : IWarehouseAssistantService
{
    private readonly WarehouseDbContext _dbContext;

    public WarehouseAssistantService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WarehouseAssistantResponse> AskAsync(WarehouseAssistantRequest request)
    {
        var question = (request.Question ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(question))
            return new WarehouseAssistantResponse
            {
                Answer = "Tôi chưa nhận được câu hỏi. Hãy hỏi về tồn kho, sản phẩm, kho, nhập/xuất, hoặc kiểm kê."
            };

        var normalized = question.ToLowerInvariant();

        if (normalized.Contains("tổng sản phẩm") || normalized.Contains("số sản phẩm") || normalized.Contains("sản phẩm"))
        {
            var count = await _dbContext.Products.CountAsync(p => !p.IsDeleted);
            return new WarehouseAssistantResponse
            {
                Answer = $"Hiện tại hệ thống đang có {count} sản phẩm đang hoạt động."
            };
        }

        if (normalized.Contains("tổng kho") || normalized.Contains("số kho") || normalized.Contains("kho"))
        {
            var count = await _dbContext.Warehouses.CountAsync(w => !w.IsDeleted);
            return new WarehouseAssistantResponse
            {
                Answer = $"Hệ thống đang quản lý {count} kho."
            };
        }

        if (normalized.Contains("sắp hết") || normalized.Contains("low stock") || normalized.Contains("thiếu hàng"))
        {
            var lowStock = await _dbContext.Inventories
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .Include(i => i.Location)
                .Where(i => !i.IsDeleted && !i.Product!.IsDeleted && i.AvailableQuantity < i.Product.MinimumStock)
                .OrderBy(i => i.AvailableQuantity)
                .Take(5)
                .Select(i => new
                {
                    Product = i.Product!.Name,
                    Warehouse = i.Warehouse!.Name,
                    Location = i.Location!.Name,
                    Available = i.AvailableQuantity,
                    Min = i.Product.MinimumStock
                })
                .ToListAsync();

            if (!lowStock.Any())
                return new WarehouseAssistantResponse
                {
                    Answer = "Hiện không có sản phẩm nào ở mức sắp hết hàng."
                };

            var lines = lowStock.Select(x =>
                $"- {x.Product} tại {x.Warehouse} / {x.Location}: còn {x.Available}, tối thiểu {x.Min}.");

            return new WarehouseAssistantResponse
            {
                Answer = "Các mặt hàng sắp hết hàng hiện tại:\n" + string.Join("\n", lines)
            };
        }

        if (normalized.Contains("nhập kho") || normalized.Contains("nhập") || normalized.Contains("receipt"))
        {
            var value = await _dbContext.GoodsReceiptDetails
                .Include(d => d.GoodsReceipt)
                .Where(d => !d.IsDeleted && !d.GoodsReceipt!.IsDeleted && d.GoodsReceipt.ConfirmedAt != null)
                .SumAsync(d => (decimal?)d.Quantity) ?? 0m;

            return new WarehouseAssistantResponse
            {
                Answer = $"Lượng hàng đã nhập được xác nhận trong hệ thống là {value:0.##} đơn vị."
            };
        }

        if (normalized.Contains("xuất kho") || normalized.Contains("xuất") || normalized.Contains("issue"))
        {
            var value = await _dbContext.GoodsIssueDetails
                .Include(d => d.GoodsIssue)
                .Where(d => !d.IsDeleted && !d.GoodsIssue!.IsDeleted && d.GoodsIssue.ConfirmedAt != null)
                .SumAsync(d => (decimal?)d.Quantity) ?? 0m;

            return new WarehouseAssistantResponse
            {
                Answer = $"Lượng hàng đã xuất được xác nhận trong hệ thống là {value:0.##} đơn vị."
            };
        }

        if (normalized.Contains("kiểm kê") || normalized.Contains("stocktake") || normalized.Contains("đếm hàng"))
        {
            var count = await _dbContext.Stocktakes.CountAsync(s => !s.IsDeleted && s.Status != "CONFIRMED" && s.Status != "CLOSED");
            return new WarehouseAssistantResponse
            {
                Answer = $"Hiện có {count} đợt kiểm kê đang chờ xử lý."
            };
        }

        if (normalized.Contains("nhật ký") || normalized.Contains("audit") || normalized.Contains("lịch sử"))
        {
            var count = await _dbContext.AuditLogs.CountAsync(a => !a.IsDeleted);
            return new WarehouseAssistantResponse
            {
                Answer = $"Hệ thống lưu trữ {count} bản ghi nhật ký hoạt động."
            };
        }

        return new WarehouseAssistantResponse
        {
            Answer = "Tôi có thể hỗ trợ về: số lượng tồn kho, mặt hàng sắp hết, nhật ký hoạt động, kiểm kê, nhập/xuất kho. Ví dụ: 'Sản phẩm nào sắp hết?', 'Có bao nhiêu kho?', 'Tổng nhập kho tháng này là bao nhiêu?'"
        };
    }
}
