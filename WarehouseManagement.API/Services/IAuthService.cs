using WarehouseManagement.API.Models.Auth;

namespace WarehouseManagement.API.Services;

public interface IAuthService
{
    Task<TokenResponse?> LoginAsync(string userName, string password);
}
