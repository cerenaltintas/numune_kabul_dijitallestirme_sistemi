using NumuneKabul.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Kestrel ve FormOptions ─────────────────────
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 52428800; // 50 MB
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50 MB
});

// ─── MVC ─────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ─── Session ───────────────────
// Güvenlik: Session cookie HttpOnly + SecurePolicy ile yapılandırıldı.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8); // JWT ExpirationHours ile uyumlu
    options.Cookie.HttpOnly = true;              // XSS koruması: JS session cookie'ye erişemez
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict; // CSRF koruması
});

// ─── IHttpContextAccessor ─────────────
builder.Services.AddHttpContextAccessor();

// ─── API İstemcisi ──────────────────────────
builder.Services.AddHttpClient<IApiClientService, NumuneKabul.Web.Services.ApiClientService>(client =>
{
    var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl")
                     ?? "http://localhost:5151/";
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromMinutes(10); // 50 MB dosyalar için timeout süresini uzat
});

var app = builder.Build();

// ─── HTTP Pipeline ────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Session middleware, Authorization'dan ÖNCE gelmelidir.
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Document}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
