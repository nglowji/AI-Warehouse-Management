namespace WarehouseManagement.API.Entities;

public class User : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? RefreshTokenId { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
