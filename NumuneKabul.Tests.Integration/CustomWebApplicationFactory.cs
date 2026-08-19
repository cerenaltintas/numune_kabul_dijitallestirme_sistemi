using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NumuneKabul.API;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Infrastructure.Data;

namespace NumuneKabul.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private System.Data.Common.DbConnection _connection;

    public CustomWebApplicationFactory()
    {
        _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DatabaseProvider", "SQLite" },
                { "ConnectionStrings:SQLite", "DataSource=file::memory:?cache=shared" }
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);
            
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            db.Database.EnsureCreated();

            // Seed admin user
            if (!db.Users.Any(u => u.Username == "admin"))
            {
                db.Users.Add(new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                    Name = "Admin User",
                    Role = "Admin"
                });
                db.SaveChanges();
            }
        });
    }
}
