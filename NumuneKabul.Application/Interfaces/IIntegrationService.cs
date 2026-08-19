using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

/// <summary>
/// �� Entegrasyon: Mock REST servise XML gönderir ve durumu takip eder.
/// </summary>
public interface IIntegrationService
{
    /// <summary>
    /// Belirtilen PDF için XML üretip mock LIS/HBYS servisine gönderir.
    /// </summary>
    Task<IntegrationJobDto> SendToMockServiceAsync(int pdfDocumentId);

    /// <summary>
    /// Belirtilen PDF'in son gönderim işinin durumunu getirir.
    /// </summary>
    Task<IntegrationJobDto?> GetJobStatusAsync(int pdfDocumentId);

    /// <summary>
    /// Başarısız olan bir gönderim işini yeniden dener (max 3 deneme).
    /// </summary>
    Task<IntegrationJobDto> RetryJobAsync(int pdfDocumentId);
}
