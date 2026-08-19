using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Application.Services;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Interfaces;
using Xunit;

namespace NumuneKabul.Tests.Unit;

public class PdfServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPdfDocumentRepository> _mockPdfRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<PdfService>> _mockLogger;
    private readonly Mock<IFileStorageService> _mockFileStorage;
    private readonly Mock<IAuditLogService> _mockAuditLogService;
    private readonly PdfService _pdfService;

    public PdfServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPdfRepo = new Mock<IPdfDocumentRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<PdfService>>();
        _mockFileStorage = new Mock<IFileStorageService>();
        _mockAuditLogService = new Mock<IAuditLogService>();

        _mockUnitOfWork.Setup(u => u.PdfDocuments).Returns(_mockPdfRepo.Object);

        _pdfService = new PdfService(_mockUnitOfWork.Object, _mockMapper.Object, _mockLogger.Object, _mockFileStorage.Object, _mockAuditLogService.Object);
    }

    [Fact]
    public async Task UploadPdfAsync_ShouldCreateFileAndDatabaseRecord()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var fileName = "test.pdf";
        int institutionId = 1;
        int? templateId = null;

        _mockFileStorage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("uploads/pdfs/test.pdf");

        _mockPdfRepo.Setup(r => r.AddAsync(It.IsAny<PdfDocument>()))
            .Callback<PdfDocument>(p => p.Id = 1)
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<PdfUploadResultDto>(It.IsAny<PdfDocument>()))
            .Returns(new PdfUploadResultDto { Id = 1, FileName = fileName });

        // Act
        var result = await _pdfService.UploadPdfAsync(stream, fileName, institutionId, templateId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.FileName.Should().Be(fileName);

        _mockPdfRepo.Verify(r => r.AddAsync(It.IsAny<PdfDocument>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeletePdfAsync_ShouldReturnFalse_WhenPdfNotFound()
    {
        // Arrange
        _mockPdfRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PdfDocument?)null);

        // Act
        var result = await _pdfService.DeletePdfAsync(99);

        // Assert
        result.Should().BeFalse();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetPdfByIdAsync_ShouldReturnNull_WhenPdfNotFound()
    {
        // Arrange
        _mockPdfRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((PdfDocument?)null);

        // Act
        var result = await _pdfService.GetPdfByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadPdfAsync_ShouldCallFileStorage_OnValidInput()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF magic bytes
        var fileName = "numune.pdf";

        _mockFileStorage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("uploads/pdfs/numune.pdf");

        _mockPdfRepo.Setup(r => r.AddAsync(It.IsAny<PdfDocument>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<PdfUploadResultDto>(It.IsAny<PdfDocument>()))
            .Returns(new PdfUploadResultDto { Id = 2, FileName = fileName });

        // Act
        var result = await _pdfService.UploadPdfAsync(stream, fileName, 1, null);

        // Assert
        _mockFileStorage.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
