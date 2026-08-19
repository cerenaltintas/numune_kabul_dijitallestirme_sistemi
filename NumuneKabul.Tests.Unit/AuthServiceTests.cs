using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Services;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Interfaces;
using System.Linq.Expressions;
using Xunit;

namespace NumuneKabul.Tests.Unit;

/// <summary>
/// AuthService unit testleri — Login akışı, JWT üretimi, güvenlik kontrolleri
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IGenericRepository<User>> _mockUserRepo;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepo = new Mock<IGenericRepository<User>>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepo.Object);

        // Test için in-memory konfigürasyon
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "JwtSettings:Secret", "TestSecretKeyForUnitTestsMinimum32Characters!" },
            { "JwtSettings:Issuer", "NumuneKabulTestAPI" },
            { "JwtSettings:Audience", "NumuneKabulTestWeb" },
            { "JwtSettings:ExpirationHours", "8" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _service = new AuthService(_mockUnitOfWork.Object, _configuration, _mockLogger.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange — Kullanıcı bulunamıyor
        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _service.LoginAsync(new LoginRequestDto
        {
            Username = "hayalet_kullanici",
            Password = "herhangi_sifre"
        });

        // Assert
        result.Should().BeNull("Kullanıcı bulunamadığında null dönmeli.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsInvalid()
    {
        // Arrange — Kullanıcı var, şifre yanlış
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("dogru_sifre");
        var user = new User { Id = 1, Username = "admin", Name = "Admin", Role = "Admin", PasswordHash = hashedPassword };

        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _service.LoginAsync(new LoginRequestDto
        {
            Username = "admin",
            Password = "yanlis_sifre"
        });

        // Assert
        result.Should().BeNull("Şifre yanlış olduğunda null dönmeli.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokenWithRole_WhenCredentialsAreValid()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("dogru_sifre");
        var user = new User
        {
            Id = 1,
            Username = "admin",
            Name = "Admin Kullanıcı",
            Role = "Admin",
            PasswordHash = hashedPassword,
            InstitutionId = 2
        };

        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _service.LoginAsync(new LoginRequestDto
        {
            Username = "admin",
            Password = "dogru_sifre"
        });

        // Assert
        result.Should().NotBeNull("Doğru kimlik bilgisiyle token dönmeli.");
        result!.Token.Should().NotBeNullOrEmpty("JWT token üretilmeli.");
        result.Role.Should().Be("Admin");
        result.Username.Should().Be("admin");
    }

    [Fact]
    public async Task GenerateToken_ShouldContainInstitutionIdClaim_WhenUserHasInstitution()
    {
        // Arrange
        var user = new User
        {
            Id = 5,
            Username = "personel",
            Name = "Test Personel",
            Role = "Staff",
            PasswordHash = "dummy",
            InstitutionId = 3
        };

        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { user });

        var result = await _service.LoginAsync(new LoginRequestDto { Username = "personel", Password = "dummy" });

        // Act
        var token = _service.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result!.Token);

        var instClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "InstitutionId");
        Assert.NotNull(instClaim);
        jwtToken.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Staff");
    }
}
