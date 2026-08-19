using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NumuneKabul.Application;
using NumuneKabul.Infrastructure;
using NumuneKabul.Infrastructure.Data;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Numune Kabul API başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Kestrel and FormOptions for 50MB uploads globally
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.Limits.MaxRequestBodySize = 52428800; // 50 MB
    });

    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 52428800; // 50 MB
    });

    // Replace default logging with Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // ─── JWT Authentication ───────────────────────────────────────────────────
    var jwtSecret = builder.Configuration["JwtSettings:Secret"]
        ?? throw new InvalidOperationException("JwtSettings:Secret yapılandırma değeri eksik!");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero  // Token süresini tam olarak uygula
            };
        });
    // ──────────────────────────────────────────────────────────────────────────

    // Add Services from other layers
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<NumuneKabul.API.Filters.RequestResponseLoggingFilter>();
    });

    // Swagger/OpenAPI setup
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Numune Kabul API",
            Version = "v1",
            Description = "Numune Kabul Dijitalleştirme Sistemi API"
        });

        // Swagger'a JWT desteği ekle
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "JWT token girin. Örnek: Bearer {token}"
        });
        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ─── CORS Konfigürasyonu ─────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        // Sadece geliştirme ortamı için: Tüm originlere izin ver
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

        // Production politikası: Yalnızca bilinen originler
        options.AddPolicy("WebApp", policy =>
            policy.WithOrigins(
                    "http://localhost:5001",
                    "https://localhost:7001",
                    "http://localhost:5000",
                    "https://localhost:7000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
    });
    // ──────────────────────────────────────────────────────────────────────────

    var app = builder.Build();

    // 1. Global Exception Middleware (EN ÜSTTE olmalı)
    app.UseMiddleware<NumuneKabul.API.Middlewares.GlobalExceptionMiddleware>();

    // Auto-create database and Seed Data (for development purposes)
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate(); // Auto-apply migrations

            // Seed Data
            await NumuneKabul.Infrastructure.Data.SeedData.InitializeAsync(context);

            Log.Information("Veritabanı kontrol edildi ve örnek veriler (Seed Data) yüklendi.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Veritabanı oluşturulurken hata oluştu.");
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Numune Kabul API v1"));
    }

    app.UseHttpsRedirection();

    // CORS — ortama göre doğru politika
    if (app.Environment.IsDevelopment())
        app.UseCors("AllowAll");
    else
        app.UseCors("WebApp");

    // ─── Authentication ÖNCE, Authorization SONRA ────────────────────────────
    app.UseAuthentication();
    app.UseAuthorization();
    // ──────────────────────────────────────────────────────────────────────────

    app.MapControllers();

    // Kök dizine (/) gelen istekleri otomatik olarak Swagger'a yönlendir
    app.MapGet("/", () => Results.Redirect("/swagger"));

    app.Run();
}
catch (Exception ex)
{
    if (ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
    {
        throw;
    }
    Log.Fatal(ex, "Uygulama başlatılırken kritik bir hata oluştu.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
