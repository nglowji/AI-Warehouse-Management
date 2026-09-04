using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Entities;
using WarehouseManagement.API.Models.Users;

namespace WarehouseManagement.API.Services;

public class UserService : IUserService
{
    private readonly WarehouseDbContext _dbContext;

    public UserService(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _dbContext.Users
            .Include(u => u.Roles)
            .Where(u => !u.IsDeleted)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _dbContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            throw new ArgumentException("UserName is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required.");

        var exists = await _dbContext.Users.AnyAsync(u => u.UserName == request.UserName || u.Email == request.Email);
        if (exists)
            throw new InvalidOperationException("User already exists.");

        var roles = await _dbContext.Roles
            .Where(r => request.Roles.Contains(r.Name))
            .ToListAsync();

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName,
            Phone = request.Phone,
            IsActive = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Roles = roles
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _dbContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user is null)
            return null;

        user.Email = request.Email;
        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.IsActive = request.IsActive;

        var roles = await _dbContext.Roles
            .Where(r => request.Roles.Contains(r.Name))
            .ToListAsync();
        user.Roles = roles;

        await _dbContext.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user is null)
        {
            return false;
        }

        user.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            IsActive = user.IsActive,
            Roles = user.Roles.Select(r => r.Name).ToList()
        };
    }
}
