using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Interfaces;

namespace NumuneKabul.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        // Kullanıcıyı username ile bul
        var users = await _unitOfWork.Users.FindAsync(u => u.Username == request.Username);
        var user = users.FirstOrDefault();

        // Zamanlama saldırılarını (Timing Attack) önlemek için:         
        bool isPasswordValid = false;
        
        if (user != null)
        {
            // Gerçek kullanıcı varsa gerçek hash ile doğrula
            isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        }
        else
        {
            // Kullanıcı yoksa, rastgele üretilmiş sahte (dummy) bir şifre hash'i ile sanki varmış gibi zaman harca
            // $2a$11$00000000000000000000000000000000000000000000000000000 geçerli bir BCrypt salt uzunluğu formatıdır.
            BCrypt.Net.BCrypt.Verify(request.Password, "$2a$11$00000000000000000000000000000000000000000000000000000");
        }

        if (user == null || !isPasswordValid)
        {
            // Dışarıya "Kullanıcı Yok" veya "Şifre Yanlış" detayı vermiyoruz. Sadece loglara yazıyoruz.
            _logger.LogWarning("Başarısız giriş denemesi. Username: {Username}", request.Username);
            return null;
        }

        var token = GenerateToken(user);
        var expirationHours = _configuration.GetValue<int>("JwtSettings:ExpirationHours", 8);

        _logger.LogInformation("Kullanıcı başarıyla giriş yaptı: {Username}, Rol: {Role}", user.Username, user.Role);

        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            Name = user.Name,
            Role = user.Role,
            InstitutionId = user.InstitutionId,
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
        };
    }

    public string GenerateToken(User user)
    {
        var secret = _configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT Secret ayarı eksik!");
        var issuer = _configuration["JwtSettings:Issuer"] ?? "NumuneKabulAPI";
        var audience = _configuration["JwtSettings:Audience"] ?? "NumuneKabulWeb";
        var expirationHours = _configuration.GetValue<int>("JwtSettings:ExpirationHours", 8);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Kullanıcının hangi hastaneye ait olduğu Token'a ekleniyor.
        if (user.InstitutionId.HasValue)
        {
            claims.Add(new Claim("InstitutionId", user.InstitutionId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
