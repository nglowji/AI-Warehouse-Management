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
        }

        await context.SaveChangesAsync();
    }
}
