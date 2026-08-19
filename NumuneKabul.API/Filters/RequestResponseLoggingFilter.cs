using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace NumuneKabul.API.Filters;

public class RequestResponseLoggingFilter : IAsyncActionFilter
{
    private readonly ILogger<RequestResponseLoggingFilter> _logger;

    public RequestResponseLoggingFilter(ILogger<RequestResponseLoggingFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // İstek gelmeden önce (Request Logging)
        var actionName = context.ActionDescriptor.DisplayName;
        var method = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path;
        
        _logger.LogInformation("HTTP {Method} isteği başlatıldı: {Path} - Action: {ActionName}", method, path, actionName);

        // Action çalıştırılıyor
        var resultContext = await next();

        // İstek bittikten sonra (Response Logging)
        if (resultContext.Exception != null)
        {
            _logger.LogError(resultContext.Exception, "Action işlenirken hata oluştu: {ActionName}", actionName);
        }
        else
        {
            _logger.LogInformation("HTTP {Method} isteği tamamlandı: {Path} - Durum Kodu: {StatusCode}", 
                method, path, context.HttpContext.Response.StatusCode);
        }
    }
}
