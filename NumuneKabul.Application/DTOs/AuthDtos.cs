namespace NumuneKabul.Application.DTOs;

// Sisteme giriş yaparken dış dünya ile haberleştiğimiz güvenli taşıma kutuları (DTO)
public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? InstitutionId { get; set; } // Hangi kuruma ait olduğu bilgisi eklendi
    public DateTime ExpiresAt { get; set; }
}
