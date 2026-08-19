using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Application.Services;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Interfaces;
using System.Linq.Expressions;
using Xunit;

namespace NumuneKabul.Tests.Unit;

/// <summary>
/// XmlService unit testleri — XML üretimi ve arşivleme
/// </summary>
public class XmlServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPdfDocumentRepository> _mockPdfDocRepo;
    private readonly Mock<IGenericRepository<OcrResult>> _mockOcrRepo;
    private readonly Mock<IGenericRepository<ExtractedField>> _mockFieldRepo;
    private readonly Mock<IGenericRepository<XmlArchive>> _mockXmlRepo;
    private readonly Mock<IXmlBuilder> _mockXmlBuilder;
    private readonly Mock<IAuditLogService> _mockAuditLog;
    private readonly Mock<ILogger<XmlService>> _mockLogger;
    private readonly XmlService _service;

    public XmlServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPdfDocRepo = new Mock<IPdfDocumentRepository>();
        _mockOcrRepo = new Mock<IGenericRepository<OcrResult>>();
        _mockFieldRepo = new Mock<IGenericRepository<ExtractedField>>();
        _mockXmlRepo = new Mock<IGenericRepository<XmlArchive>>();
        _mockXmlBuilder = new Mock<IXmlBuilder>();
        _mockAuditLog = new Mock<IAuditLogService>();
        _mockLogger = new Mock<ILogger<XmlService>>();

        _mockUnitOfWork.Setup(u => u.PdfDocuments).Returns(_mockPdfDocRepo.Object);
        _mockUnitOfWork.Setup(u => u.OcrResults).Returns(_mockOcrRepo.Object);
        _mockUnitOfWork.Setup(u => u.ExtractedFields).Returns(_mockFieldRepo.Object);
        _mockUnitOfWork.Setup(u => u.XmlArchives).Returns(_mockXmlRepo.Object);

        // XmlService constructor sırası: IUnitOfWork, IAuditLogService, IXmlBuilder, ILogger
        _service = new XmlService(
            _mockUnitOfWork.Object,
            _mockAuditLog.Object,
            _mockXmlBuilder.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetByPdfIdAsync_ShouldReturnNull_WhenNoArchiveExists()
    {
        // Arrange
        _mockXmlRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<XmlArchive, bool>>>()))
            .ReturnsAsync(new List<XmlArchive>());

        // Act
        var result = await _service.GetByPdfIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPdfIdAsync_ShouldReturnLatest_WhenMultipleArchivesExist()
    {
        // Arrange
        var archives = new List<XmlArchive>
        {
            new() { Id = 1, PdfDocumentId = 5, XmlContent = "<old/>", CreatedDate = DateTime.UtcNow.AddDays(-1) },
            new() { Id = 2, PdfDocumentId = 5, XmlContent = "<new/>", CreatedDate = DateTime.UtcNow }
        };

        _mockXmlRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<XmlArchive, bool>>>()))
            .ReturnsAsync(archives);

        // Act
        var result = await _service.GetByPdfIdAsync(5);

        // Assert — XmlService XmlArchive -> XmlArchiveDto dönüşümü yapıyor
        result.Should().NotBeNull();
        result!.XmlContent.Should().Be("<new/>");
    }

    [Fact]
    public async Task CreateAndSaveAsync_ShouldThrowKeyNotFoundException_WhenPdfNotFound()
    {
        // Arrange
        _mockPdfDocRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PdfDocument?)null);

        // Act
        Func<Task> act = async () => await _service.CreateAndSaveAsync(999);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
