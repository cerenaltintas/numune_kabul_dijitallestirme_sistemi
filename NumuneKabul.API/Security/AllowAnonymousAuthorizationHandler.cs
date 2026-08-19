#if DEBUG
// ╔══════════════════════════════════════════════════════════════════════════════╗
// ║  GÜVENLİK UYARISI (Defense in Depth):                                      ║
// ║  Bu sınıf YALNIZCA DEBUG derlemesinde mevcuttur.                            ║
// ║  Production build'inde bu dosya DERLENMEZ ve DI'ya eklenemez.               ║
// ║  Bu sınıf DI'ya kaydedildiğinde TÜM [Authorize] etiketlerini bypass eder.  ║
// ║  Sadece Swagger/Postman ile hızlı test amaçlıdır.                          ║
// ╚══════════════════════════════════════════════════════════════════════════════╝

using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace NumuneKabul.API.Security;

/// <summary>
/// Geliştirme (Development) ortamında Authorize etiketlerini by-pass etmek için kullanılır.
/// Tüm yetkilendirme kurallarını otomatik olarak 'Başarılı' (Succeed) sayar.
/// 
/// DI Kaydı (SADECE DEBUG'da kullanın):
/// builder.Services.AddSingleton&lt;IAuthorizationHandler, AllowAnonymousAuthorizationHandler&gt;();
/// </summary>
public class AllowAnonymousAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.PendingRequirements.ToList())
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
#endif
