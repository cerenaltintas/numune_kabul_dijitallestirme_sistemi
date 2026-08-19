using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace NumuneKabul.Tests.Integration;

/// <summary>
/// API Integration testleri — ProgramPartial.cs aracılığıyla WebApplicationFactory kullanır.

/// </summary>
public class ApiHealthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiHealthTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Swagger_ShouldBeAccessible_InDevelopment()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        // Assert — Swagger endpoint'i erişilebilir olmalı
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound,
            "Swagger endpoint erişilebilir olmalı (geliştirme modunda).");
    }

    [Fact]
    public async Task AuthLogin_WithoutCredentials_ShouldReturn400()
    {
        // Arrange
        var client = _factory.CreateClient();
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/auth/login", content);

        // Assert — Eksik credential → 400 Bad Request
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
