using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Application.Services;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Enums;
using NumuneKabul.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using System.Linq.Expressions;
using Xunit;

namespace NumuneKabul.Tests.Unit;

/// <summary>
/// ExtractedFieldService unit testleri — Manuel alan düzeltme ve audit log
/// </summary>
public class ExtractedFieldServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IGenericRepository<ExtractedField>> _mockFieldRepo;
    private readonly Mock<IPdfDocumentRepository> _mockPdfRepo;
    private readonly Mock<IAuditLogService> _mockAuditLogService;
    private readonly ExtractedFieldService _service;

    public ExtractedFieldServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockFieldRepo = new Mock<IGenericRepository<ExtractedField>>();
        _mockPdfRepo = new Mock<IPdfDocumentRepository>();
        _mockAuditLogService = new Mock<IAuditLogService>();

        _mockUnitOfWork.Setup(u => u.ExtractedFields).Returns(_mockFieldRepo.Object);
        _mockUnitOfWork.Setup(u => u.PdfDocuments).Returns(_mockPdfRepo.Object);

        _service = new ExtractedFieldService(_mockUnitOfWork.Object, _mockAuditLogService.Object);
    }

    [Fact]
    public async Task UpdateFieldAsync_ShouldReturnFalse_WhenFieldNotFound()
    {
        // Arrange
        _mockFieldRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ExtractedField, bool>>>()))
            .ReturnsAsync(new List<ExtractedField>());

        // Act
        var result = await _service.UpdateFieldAsync(999, new UpdateExtractedFieldDto
        {
            Id = 999,
            CorrectedValue = "Test"
        }, 1);

        // Assert
        result.Should().BeFalse();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateFieldAsync_ShouldUpdateAndWriteAuditLog_WhenFieldExists()
    {
        // Arrange
        var existingField = new ExtractedField
        {
            Id = 1,
            FieldName = "HastaAdi",
            RawValue = "Ahmet Yilmaz",
            PdfDocumentId = 5
        };
        _mockFieldRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ExtractedField, bool>>>()))
            .ReturnsAsync(new List<ExtractedField> { existingField });

        _mockPdfRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((PdfDocument?)null); 

        _mockAuditLogService.Setup(s => s.LogAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateFieldAsync(1, new UpdateExtractedFieldDto
        {
            Id = 1,
            CorrectedValue = "Ahmet Yılmaz",
            Notes = "Türkçe karakter düzeltmesi"
        }, 1);

        // Assert
        result.Should().BeTrue();
        existingField.CorrectedValue.Should().Be("Ahmet Yılmaz");
        existingField.Status.Should().Be(DocumentStatus.Corrected.ToString());

        _mockAuditLogService.Verify(s => s.LogAsync(
            "ManualFieldCorrection",
            It.IsAny<string>(),
            "PdfDocument",
            "5",
            "Info",
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
