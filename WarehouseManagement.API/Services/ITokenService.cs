namespace WarehouseManagement.API.Services;

public interface ITokenService
{
    string GenerateAccessToken(string userName, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
    DateTime GetRefreshTokenExpiry();
}
