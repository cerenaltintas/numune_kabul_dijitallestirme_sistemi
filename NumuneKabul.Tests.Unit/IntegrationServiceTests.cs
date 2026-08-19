using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Application.Services;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Enums;
using NumuneKabul.Domain.Interfaces;
using System.Linq.Expressions;
using Xunit;

namespace NumuneKabul.Tests.Unit;

/// <summary>
/// IntegrationService unit testleri — Mock REST gönderim, hata yönetimi, retry limiti
/// </summary>
public class IntegrationServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPdfDocumentRepository> _mockPdfRepo;
    private readonly Mock<IGenericRepository<IntegrationJob>> _mockJobRepo;
    private readonly Mock<IXmlService> _mockXmlService;
    private readonly Mock<IXmlMappingService> _mockXmlMappingService;
    private readonly Mock<IIntegrationAdapter> _mockAdapter;
    private readonly Mock<IAuditLogService> _mockAuditLog;
    private readonly Mock<ILogger<IntegrationService>> _mockLogger;
    private readonly IntegrationService _service;

    public IntegrationServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPdfRepo = new Mock<IPdfDocumentRepository>();
        _mockJobRepo = new Mock<IGenericRepository<IntegrationJob>>();
        _mockXmlService = new Mock<IXmlService>();
        _mockXmlMappingService = new Mock<IXmlMappingService>();
        _mockAdapter = new Mock<IIntegrationAdapter>();
        _mockAuditLog = new Mock<IAuditLogService>();
        _mockLogger = new Mock<ILogger<IntegrationService>>();

        _mockUnitOfWork.Setup(u => u.PdfDocuments).Returns(_mockPdfRepo.Object);
        _mockUnitOfWork.Setup(u => u.IntegrationJobs).Returns(_mockJobRepo.Object);

        _service = new IntegrationService(
            _mockUnitOfWork.Object,
            _mockXmlService.Object,
            _mockXmlMappingService.Object,
            _mockAdapter.Object,
            _mockAuditLog.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SendToMockServiceAsync_ShouldThrowKeyNotFoundException_WhenPdfNotFound()
    {
        // Arrange
        _mockPdfRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PdfDocument?)null);

        // Act
        Func<Task> act = async () => await _service.SendToMockServiceAsync(999);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SendToMockServiceAsync_ShouldSetJobStatusToSent_WhenAdapterSucceeds()
    {
        // Arrange
        var pdf = new PdfDocument { Id = 1, Status = DocumentStatus.Corrected.ToString() };
        // IXmlService.GetByPdfIdAsync → XmlArchiveDto? döndürür
        var xmlArchiveDto = new NumuneKabul.Application.DTOs.XmlArchiveDto { Id = 1, PdfDocumentId = 1, XmlContent = "<xml/>" };
        var capturedJob = new IntegrationJob();

        _mockPdfRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pdf);
        _mockXmlService.Setup(s => s.GetByPdfIdAsync(1)).ReturnsAsync(xmlArchiveDto);
        _mockXmlMappingService.Setup(s => s.MapToTargetFormat(It.IsAny<string>(), It.IsAny<string>())).Returns("<mapped/>");
        _mockAdapter.Setup(a => a.SendAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(true);

        _mockJobRepo.Setup(r => r.AddAsync(It.IsAny<IntegrationJob>()))
            .Callback<IntegrationJob>(j => { j.Id = 10; capturedJob = j; })
            .Returns(Task.CompletedTask);

        _mockAuditLog.Setup(a => a.LogAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SendToMockServiceAsync(1);

        // Assert
        result.Should().NotBeNull();
        capturedJob.Status.Should().Be(IntegrationStatus.Sent.ToString());
        pdf.Status.Should().Be(DocumentStatus.IntegrationSent.ToString());
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RetryJobAsync_ShouldThrowInvalidOperationException_WhenMaxRetryExceeded()
    {
        // Arrange
        var exhaustedJob = new IntegrationJob
        {
            Id = 1,
            PdfDocumentId = 5,
            RetryCount = 3, // MaxRetryCount = 3
            Status = IntegrationStatus.Failed.ToString()
        };

        _mockJobRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<IntegrationJob, bool>>>()))
            .ReturnsAsync(new List<IntegrationJob> { exhaustedJob });

        // Act
        Func<Task> act = async () => await _service.RetryJobAsync(5);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*3*"); // Mesaj max retry sayısını içermeli
    }
}
