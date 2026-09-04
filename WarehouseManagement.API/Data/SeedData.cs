using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Entities;

namespace WarehouseManagement.API.Data;

public static class SeedData
{
    public static async Task InitializeAsync(WarehouseDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            await context.Roles.AddRangeAsync(
                new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "System administrator" },
                new Role { Id = Guid.NewGuid(), Name = "Manager", Description = "Warehouse manager" },
                new Role { Id = Guid.NewGuid(), Name = "Staff", Description = "Warehouse staff" }
            );
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
