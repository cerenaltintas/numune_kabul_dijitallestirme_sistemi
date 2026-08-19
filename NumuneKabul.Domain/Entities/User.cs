namespace NumuneKabul.Domain.Entities;

public class User //kullanıcılar
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    
    // Multi-tenant yapı için kurum bağlantısı (Admin için null olabilir)
    public int? InstitutionId { get; set; }
    public virtual Institution? Institution { get; set; }
    
    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
