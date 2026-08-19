using Microsoft.AspNetCore.Mvc;
using NumuneKabul.Web.Models;
using NumuneKabul.Web.Services;

namespace NumuneKabul.Web.Controllers;

/// <summary>
/// Kullanıcı kimlik doğrulama akışını yönetir: Login ve Logout.
/// 
/// Güvenlik Notları:
/// - JWT token yalnızca Session'da tutulur, cookie veya URL'de gönderilmez.
/// - Başarısız login'de kullanıcıya "Kullanıcı yok" / "Şifre yanlış" ayrımı yapılmaz
///   (User Enumeration koruması — API katmanıyla tutarlı).
/// - AntiForgeryToken tüm POST action'larında zorunludur.
/// </summary>
public class AccountController : Controller
{
    private readonly IApiClientService _apiClient;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IApiClientService apiClient, ILogger<AccountController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Zaten giriş yapmış kullanıcıyı yönlendir
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Index", "Document");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _apiClient.LoginAsync(model.Username, model.Password);

        if (result == null)
        {
            // GÜVENLİK: "Kullanıcı adı veya şifre hatalı" — ikisini ayırt etmiyoruz.
            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
            _logger.LogWarning("Başarısız web giriş denemesi. Kullanıcı: {Username}", model.Username);
            return View(model);
        }

        // JWT token ve kullanıcı bilgilerini Session'a yaz
        HttpContext.Session.SetString("JwtToken", result.Token);
        HttpContext.Session.SetString("UserName", result.Name);
        HttpContext.Session.SetString("UserRole", result.Role);
        HttpContext.Session.SetString("Username", result.Username);

        _logger.LogInformation("Web giriş başarılı. Kullanıcı: {Username}, Rol: {Role}", result.Username, result.Role);

        // ReturnUrl güvenlik kontrolü: sadece aynı uygulamaya ait URL'lere yönlendir
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Document");
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        _logger.LogInformation("Kullanıcı çıkış yaptı: {Username}", HttpContext.Session.GetString("Username") ?? "bilinmiyor");
        return RedirectToAction(nameof(Login));
    }

    // GET: /Account/AccessDenied
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
