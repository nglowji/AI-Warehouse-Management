using Microsoft.EntityFrameworkCore;
using WarehouseManagement.API.Data;
using WarehouseManagement.API.Models.Auth;

namespace WarehouseManagement.API.Services;

public class AuthService : IAuthService
{
    private readonly WarehouseDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public AuthService(WarehouseDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<TokenResponse?> LoginAsync(string userName, string password)
    {
        var user = await _dbContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserName == userName && u.IsActive);

        if (user is null)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }

        var roles = user.Roles.Select(r => r.Name).ToList();
        var accessToken = _tokenService.GenerateAccessToken(user.UserName, user.Email, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshTokenId = Guid.NewGuid();
        user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiry();

        await _dbContext.SaveChangesAsync();

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }
}
