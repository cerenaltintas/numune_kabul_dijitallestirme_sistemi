using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NumuneKabul.Web.Filters;

/// <summary>
/// Web arayüzünde JWT token kontrolü yapan ActionFilter.
/// Session'da JwtToken yoksa kullanıcıyı Login sayfasına yönlendirir.
/// </summary>
public class SessionAuthorizeAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Virgülle ayrılmış roller (Örn: "Admin,Numune Kabul Personeli")
    /// </summary>
    public string? Roles { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var token = context.HttpContext.Session.GetString("JwtToken");
        var userRole = context.HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(token))
        {
            // Token yoksa Account/Login'e yönlendir
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // Eğer spesifik bir Rol kısıtlaması varsa kontrol et
        if (!string.IsNullOrWhiteSpace(Roles))
        {
            var allowedRoles = Roles.Split(',').Select(r => r.Trim()).ToList();
            if (string.IsNullOrEmpty(userRole) || !allowedRoles.Contains(userRole))
            {
                // Yetki yoksa AccessDenied (veya Home) sayfasına yönlendir
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}
