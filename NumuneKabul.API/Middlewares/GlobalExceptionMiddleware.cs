using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NumuneKabul.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Uygulamada beklenmeyen bir hata oluştu: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, _env.IsDevelopment());
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Sunucu tarafında beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.",
            // Production'da hata detayını gizleyerek güvenlik açığı önlenir
            Detail = isDevelopment ? exception.Message : null
        };

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}
