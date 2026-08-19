using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NumuneKabul.API;
using NumuneKabul.Application.DTOs;
using Xunit;

namespace NumuneKabul.Tests.Integration;

public class PdfControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PdfControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var loginDto = new LoginRequestDto { Username = "admin", Password = "password" };
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
    }

    [Fact]
    public async Task GetAllPdfs_ReturnsOkAndJson()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/Pdf");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString().Should().Contain("application/json");
        
        var pdfs = await response.Content.ReadFromJsonAsync<IEnumerable<PdfDocumentDto>>();
        pdfs.Should().NotBeNull();
    }
}
