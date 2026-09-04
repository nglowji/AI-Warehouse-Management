using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Entities;
using WarehouseManagement.API.Models.GoodsIssues;

namespace WarehouseManagement.API.Services;

public class GoodsIssueService : IGoodsIssueService
{
    private readonly WarehouseDbContext _dbContext;

    public GoodsIssueService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GoodsIssueDto>> GetAllAsync()
    {
        var issues = await _dbContext.GoodsIssues
            .Include(i => i.Warehouse)
            .Include(i => i.Details)
                .ThenInclude(d => d.Product)
            .Include(i => i.Details)
                .ThenInclude(d => d.Location)
            .Where(i => !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return issues.Select(MapToDto).ToList();
    }

    public async Task<GoodsIssueDto?> GetByIdAsync(Guid id)
    {
        var issue = await _dbContext.GoodsIssues
            .Include(i => i.Warehouse)
            .Include(i => i.Details)
                .ThenInclude(d => d.Product)
            .Include(i => i.Details)
                .ThenInclude(d => d.Location)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

        return issue is null ? null : MapToDto(issue);
    }

    public async Task<GoodsIssueDto> CreateAsync(CreateGoodsIssueRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Issue code is required.");

        if (request.WarehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse is required.");

        var warehouseExists = await _dbContext.Warehouses.AnyAsync(w => w.Id == request.WarehouseId && !w.IsDeleted);
        if (!warehouseExists)
            throw new InvalidOperationException("Warehouse not found.");

        if (request.Details is null || !request.Details.Any())
            throw new InvalidOperationException("At least one issue detail is required.");

        foreach (var detail in request.Details)
        {
            if (detail.ProductId == Guid.Empty)
                throw new ArgumentException("Product is required.");

            if (detail.LocationId == Guid.Empty)
                throw new ArgumentException("Location is required.");

            if (detail.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            var productExists = await _dbContext.Products.AnyAsync(p => p.Id == detail.ProductId && !p.IsDeleted);
            if (!productExists)
                throw new InvalidOperationException($"Product {detail.ProductId} not found.");

            var locationExists = await _dbContext.WarehouseLocations.AnyAsync(l =>
                l.Id == detail.LocationId && l.WarehouseId == request.WarehouseId && !l.IsDeleted);
            if (!locationExists)
                throw new InvalidOperationException($"Location {detail.LocationId} not found for warehouse.");

            var inventory = await _dbContext.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == detail.ProductId
                    && i.WarehouseId == request.WarehouseId
                    && i.LocationId == detail.LocationId
                    && !i.IsDeleted);

            if (inventory is null || inventory.Quantity < detail.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product {detail.ProductId} at location {detail.LocationId}.");
        }

        var issue = new GoodsIssue
        {
            Code = request.Code.Trim(),
            WarehouseId = request.WarehouseId,
            Status = request.Status,
            CreatedByUserId = request.CreatedByUserId,
            Details = new List<GoodsIssueDetail>()
        };

        foreach (var detail in request.Details)
        {
            issue.Details.Add(new GoodsIssueDetail
            {
                ProductId = detail.ProductId,
                LocationId = detail.LocationId,
                Quantity = detail.Quantity
            });
        }

        _dbContext.GoodsIssues.Add(issue);
        await _dbContext.SaveChangesAsync();

        var created = await _dbContext.GoodsIssues
            .Include(i => i.Warehouse)
            .Include(i => i.Details)
                .ThenInclude(d => d.Product)
            .Include(i => i.Details)
                .ThenInclude(d => d.Location)
            .FirstAsync(i => i.Id == issue.Id);

        return MapToDto(created);
    }

    public async Task<GoodsIssueDto?> UpdateAsync(Guid id, UpdateGoodsIssueRequest request)
    {
        var issue = await _dbContext.GoodsIssues
            .Include(i => i.Warehouse)
            .Include(i => i.Details)
                .ThenInclude(d => d.Product)
            .Include(i => i.Details)
                .ThenInclude(d => d.Location)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

        if (issue is null)
            return null;

        if (request.Code is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                throw new ArgumentException("Issue code is required.");

            issue.Code = request.Code.Trim();
        }

        if (request.WarehouseId.HasValue)
        {
            if (request.WarehouseId.Value == Guid.Empty)
                throw new ArgumentException("Warehouse is required.");

            var warehouseExists = await _dbContext.Warehouses.AnyAsync(w => w.Id == request.WarehouseId.Value && !w.IsDeleted);
            if (!warehouseExists)
                throw new InvalidOperationException("Warehouse not found.");

            issue.WarehouseId = request.WarehouseId.Value;
        }

        if (request.Status is not null)
            issue.Status = request.Status.Trim();

        if (request.ConfirmedByUserId.HasValue)
            issue.ConfirmedByUserId = request.ConfirmedByUserId.Value;

        if (request.Details is not null)
        {
            foreach (var detail in issue.Details)
            {
                _dbContext.GoodsIssueDetails.Remove(detail);
            }

            issue.Details.Clear();

            foreach (var detail in request.Details)
            {
                if (detail.ProductId is null || detail.ProductId == Guid.Empty)
                    throw new ArgumentException("Product is required.");

                if (detail.LocationId is null || detail.LocationId == Guid.Empty)
                    throw new ArgumentException("Location is required.");

                if (detail.Quantity is null || detail.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero.");

                issue.Details.Add(new GoodsIssueDetail
                {
                    GoodsIssueId = issue.Id,
                    ProductId = detail.ProductId.Value,
                    LocationId = detail.LocationId.Value,
                    Quantity = detail.Quantity.Value
                });
            }
        }

        issue.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(issue);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var issue = await _dbContext.GoodsIssues.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        if (issue is null)
            return false;

        issue.IsDeleted = true;
        issue.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static GoodsIssueDto MapToDto(GoodsIssue issue)
    {
        return new GoodsIssueDto
        {
            Id = issue.Id,
            Code = issue.Code,
            WarehouseId = issue.WarehouseId,
            WarehouseName = issue.Warehouse?.Name,
            Status = issue.Status,
            CreatedByUserId = issue.CreatedByUserId,
            ConfirmedByUserId = issue.ConfirmedByUserId,
            ConfirmedAt = issue.ConfirmedAt,
            Details = issue.Details
                .Where(d => !d.IsDeleted)
                .Select(d => new GoodsIssueDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name,
                    ProductSku = d.Product?.Sku,
                    LocationId = d.LocationId,
                    LocationName = d.Location?.Name,
                    Quantity = d.Quantity
                }).ToList()
        };
    }
}
