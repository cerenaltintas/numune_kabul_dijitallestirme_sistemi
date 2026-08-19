namespace NumuneKabul.Web.Models;

/// <summary>
/// Login ekranı form view modeli.
/// Tasarım: DataAnnotations ile server-side validasyon.
/// </summary>
public class LoginViewModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [System.ComponentModel.DataAnnotations.StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Şifre zorunludur.")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Başarısız login sonrası orijinal URL'e geri dön.</summary>
    public string? ReturnUrl { get; set; }
}
