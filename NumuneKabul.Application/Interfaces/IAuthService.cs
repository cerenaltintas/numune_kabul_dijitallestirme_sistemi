using NumuneKabul.Application.DTOs;
using NumuneKabul.Domain.Entities;

namespace NumuneKabul.Application.Interfaces;

// Sistemin Kimlik Doğrulama ve JWT Token üretim süreçlerini yöneten servis sözleşmesi
public interface IAuthService
{
    /// <summary>
    /// Kullanıcı adı ve şifresiyle giriş yapar, başarılıysa JWT token döner.
    /// </summary>
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Verilen User entity'si için JWT token üretir.
    /// </summary>
    string GenerateToken(User user);
}
