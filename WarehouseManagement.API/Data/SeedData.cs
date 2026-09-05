using System.Data;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Entities;

namespace WarehouseManagement.API.Data;

public static class SeedData
{
    public static async Task InitializeAsync(WarehouseDbContext context)
    {
        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name <> '__EFMigrationsHistory';";

        var tableCount = Convert.ToInt32(await command.ExecuteScalarAsync() ?? 0);

        if (tableCount == 0)
        {
            await context.Database.EnsureCreatedAsync();
        }

        if (!await context.Roles.AnyAsync())
        {
            await context.Roles.AddRangeAsync(
                new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "System administrator" },
                new Role { Id = Guid.NewGuid(), Name = "Manager", Description = "Warehouse manager" },
                new Role { Id = Guid.NewGuid(), Name = "Staff", Description = "Warehouse staff" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Email = "admin@warehouse.local",
                FullName = "System Administrator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                IsActive = true,
                Roles = new List<Role> { adminRole }
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        if (!await context.Categories.AnyAsync())
        {
            await SeedWarehouseSampleDataAsync(context);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedWarehouseSampleDataAsync(WarehouseDbContext context)
    {
        var now = DateTime.UtcNow;
        var adminUserId = await context.Users
            .Where(u => u.UserName == "admin")
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync();

        var electronics = new Category { Id = Guid.NewGuid(), Name = "Electronics", Description = "Electronic components and devices" };
        var packaging = new Category { Id = Guid.NewGuid(), Name = "Packaging", Description = "Packing and shipping materials" };
        var safety = new Category { Id = Guid.NewGuid(), Name = "Safety Equipment", Description = "Warehouse safety supplies" };

        var units = new[]
        {
            new Unit { Id = Guid.NewGuid(), Name = "Piece", ShortName = "pcs" },
            new Unit { Id = Guid.NewGuid(), Name = "Box", ShortName = "box" },
            new Unit { Id = Guid.NewGuid(), Name = "Roll", ShortName = "roll" }
        };

        var suppliers = new[]
        {
            new Supplier { Id = Guid.NewGuid(), Name = "Northwind Components", Phone = "0901001001", Email = "sales@northwind.local", Address = "District 1, Ho Chi Minh City" },
            new Supplier { Id = Guid.NewGuid(), Name = "Saigon Packaging Co.", Phone = "0902002002", Email = "orders@saigonpack.local", Address = "Tan Binh, Ho Chi Minh City" },
            new Supplier { Id = Guid.NewGuid(), Name = "Mekong Safety Supply", Phone = "0903003003", Email = "support@mekongsafety.local", Address = "Binh Thanh, Ho Chi Minh City" }
        };

        var mainWarehouse = new Warehouse { Id = Guid.NewGuid(), Code = "WH-HCM-01", Name = "Ho Chi Minh Main Warehouse", Address = "District 7, Ho Chi Minh City" };
        var backupWarehouse = new Warehouse { Id = Guid.NewGuid(), Code = "WH-HCM-02", Name = "Binh Tan Backup Warehouse", Address = "Binh Tan, Ho Chi Minh City" };

        var locations = new[]
        {
            new WarehouseLocation { Id = Guid.NewGuid(), WarehouseId = mainWarehouse.Id, Code = "A-01", Name = "Aisle A - Rack 01" },
            new WarehouseLocation { Id = Guid.NewGuid(), WarehouseId = mainWarehouse.Id, Code = "A-02", Name = "Aisle A - Rack 02" },
            new WarehouseLocation { Id = Guid.NewGuid(), WarehouseId = mainWarehouse.Id, Code = "B-01", Name = "Aisle B - Rack 01" },
            new WarehouseLocation { Id = Guid.NewGuid(), WarehouseId = backupWarehouse.Id, Code = "C-01", Name = "Reserve Zone C01" },
            new WarehouseLocation { Id = Guid.NewGuid(), WarehouseId = backupWarehouse.Id, Code = "C-02", Name = "Reserve Zone C02" }
        };

        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Sku = "ELEC-SEN-001", Barcode = "893850100001", Name = "IoT Temperature Sensor", CategoryId = electronics.Id, SupplierId = suppliers[0].Id, UnitId = units[0].Id, CostPrice = 180000, SellingPrice = 245000, MinimumStock = 30 },
            new Product { Id = Guid.NewGuid(), Sku = "ELEC-GTW-002", Barcode = "893850100002", Name = "Warehouse Gateway Hub", CategoryId = electronics.Id, SupplierId = suppliers[0].Id, UnitId = units[0].Id, CostPrice = 1250000, SellingPrice = 1690000, MinimumStock = 12 },
            new Product { Id = Guid.NewGuid(), Sku = "PACK-BOX-010", Barcode = "893850100010", Name = "Standard Shipping Box M", CategoryId = packaging.Id, SupplierId = suppliers[1].Id, UnitId = units[1].Id, CostPrice = 8500, SellingPrice = 12000, MinimumStock = 200 },
            new Product { Id = Guid.NewGuid(), Sku = "PACK-TAPE-011", Barcode = "893850100011", Name = "Clear Packing Tape", CategoryId = packaging.Id, SupplierId = suppliers[1].Id, UnitId = units[2].Id, CostPrice = 16000, SellingPrice = 22000, MinimumStock = 80 },
            new Product { Id = Guid.NewGuid(), Sku = "SAFE-GLOVE-020", Barcode = "893850100020", Name = "Cut Resistant Gloves", CategoryId = safety.Id, SupplierId = suppliers[2].Id, UnitId = units[0].Id, CostPrice = 42000, SellingPrice = 65000, MinimumStock = 60 },
            new Product { Id = Guid.NewGuid(), Sku = "SAFE-VEST-021", Barcode = "893850100021", Name = "Reflective Safety Vest", CategoryId = safety.Id, SupplierId = suppliers[2].Id, UnitId = units[0].Id, CostPrice = 55000, SellingPrice = 78000, MinimumStock = 45 }
        };

        var inventories = new[]
        {
            new Inventory { Id = Guid.NewGuid(), ProductId = products[0].Id, WarehouseId = mainWarehouse.Id, LocationId = locations[0].Id, Quantity = 24, ReservedQuantity = 3 },
            new Inventory { Id = Guid.NewGuid(), ProductId = products[1].Id, WarehouseId = mainWarehouse.Id, LocationId = locations[1].Id, Quantity = 17, ReservedQuantity = 2 },
            new Inventory { Id = Guid.NewGuid(), ProductId = products[2].Id, WarehouseId = mainWarehouse.Id, LocationId = locations[2].Id, Quantity = 340, ReservedQuantity = 25 },
            new Inventory { Id = Guid.NewGuid(), ProductId = products[3].Id, WarehouseId = backupWarehouse.Id, LocationId = locations[3].Id, Quantity = 58, ReservedQuantity = 4 },
            new Inventory { Id = Guid.NewGuid(), ProductId = products[4].Id, WarehouseId = backupWarehouse.Id, LocationId = locations[4].Id, Quantity = 110, ReservedQuantity = 15 },
            new Inventory { Id = Guid.NewGuid(), ProductId = products[5].Id, WarehouseId = mainWarehouse.Id, LocationId = locations[2].Id, Quantity = 31, ReservedQuantity = 0 }
        };

        var receipt = new GoodsReceipt
        {
            Id = Guid.NewGuid(),
            Code = $"GR-{now:yyyyMM}-001",
            SupplierId = suppliers[0].Id,
            WarehouseId = mainWarehouse.Id,
            Status = "CONFIRMED",
            CreatedByUserId = adminUserId,
            ConfirmedByUserId = adminUserId,
            ConfirmedAt = now.AddDays(-5)
        };
        receipt.Details.Add(new GoodsReceiptDetail { Id = Guid.NewGuid(), GoodsReceiptId = receipt.Id, ProductId = products[0].Id, LocationId = locations[0].Id, Quantity = 40, UnitPrice = products[0].CostPrice });
        receipt.Details.Add(new GoodsReceiptDetail { Id = Guid.NewGuid(), GoodsReceiptId = receipt.Id, ProductId = products[1].Id, LocationId = locations[1].Id, Quantity = 15, UnitPrice = products[1].CostPrice });

        var issue = new GoodsIssue
        {
            Id = Guid.NewGuid(),
            Code = $"GI-{now:yyyyMM}-001",
            WarehouseId = mainWarehouse.Id,
            Status = "CONFIRMED",
            CreatedByUserId = adminUserId,
            ConfirmedByUserId = adminUserId,
            ConfirmedAt = now.AddDays(-2)
        };
        issue.Details.Add(new GoodsIssueDetail { Id = Guid.NewGuid(), GoodsIssueId = issue.Id, ProductId = products[2].Id, LocationId = locations[2].Id, Quantity = 45 });
        issue.Details.Add(new GoodsIssueDetail { Id = Guid.NewGuid(), GoodsIssueId = issue.Id, ProductId = products[0].Id, LocationId = locations[0].Id, Quantity = 16 });

        var stocktake = new Stocktake
        {
            Id = Guid.NewGuid(),
            Code = $"ST-{now:yyyyMM}-001",
            WarehouseId = mainWarehouse.Id,
            Status = "DRAFT",
            CreatedByUserId = adminUserId
        };
        stocktake.Details.Add(new StocktakeDetail { Id = Guid.NewGuid(), StocktakeId = stocktake.Id, ProductId = products[0].Id, LocationId = locations[0].Id, SystemQuantity = 21, ActualQuantity = 0 });

        await context.Categories.AddRangeAsync(electronics, packaging, safety);
        await context.Units.AddRangeAsync(units);
        await context.Suppliers.AddRangeAsync(suppliers);
        await context.Warehouses.AddRangeAsync(mainWarehouse, backupWarehouse);
        await context.WarehouseLocations.AddRangeAsync(locations);
        await context.Products.AddRangeAsync(products);
        await context.Inventories.AddRangeAsync(inventories);
        await context.GoodsReceipts.AddAsync(receipt);
        await context.GoodsIssues.AddAsync(issue);
        await context.Stocktakes.AddAsync(stocktake);
    }
}
