using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

/// <summary>
/// Her belge için XML üretir, arşivler ve getirir.
/// </summary>
public interface IXmlService
{
    /// <summary>
    /// Belirtilen PDF için XML üretir ve veritabanına kaydeder.
    /// </summary>
    Task<XmlCreateResultDto> CreateAndSaveAsync(int pdfDocumentId);

    /// <summary>
    /// Belirtilen PDF için daha önce kaydedilmiş XML arşivini getirir.
    /// </summary>
    Task<XmlArchiveDto?> GetByPdfIdAsync(int pdfDocumentId);
}
