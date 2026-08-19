using NumuneKabul.Application.DTOs;
using NumuneKabul.Web.Models;

namespace NumuneKabul.Web.Services;

/// <summary>
/// Web katmanı controller'larını concrete ApiClientService'e bağlamak yerine
/// bu interface'e bağlar. Test edilebilirliği artırır.
/// </summary>
public interface IApiClientService
{
    // â”€â”€â”€ Auth â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<LoginResponseViewModel?> LoginAsync(string username, string password);

    // â”€â”€â”€ Lookup & Reference Data â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<IEnumerable<InstitutionViewModel>> GetInstitutionsAsync();
    Task<IEnumerable<FormTemplateViewModel>> GetTemplatesByInstitutionAsync(int institutionId);

    // â”€â”€â”€ PDF â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<PaginatedResultViewModel<PdfDocumentViewModel>?> GetPdfsPaginatedAsync(int page, int pageSize);
    Task<PdfDocumentViewModel?> GetPdfByIdAsync(int id);
    Task<int?> UploadPdfAsync(PdfUploadViewModel model);
    Task<bool> DeletePdfAsync(int id);
    Task<Stream?> GetPdfStreamAsync(int id);

    // â”€â”€â”€ OCR â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<OcrResultViewModel?> GetOcrResultAsync(int id);
    Task<bool> StartOcrAsync(int id);
    Task<bool> ApplyTemplateAsync(int pdfId, int institutionId, int templateId);

    // â”€â”€â”€ Form Template â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<IEnumerable<FormTemplateViewModel>> GetAllFormTemplatesAsync();
    Task<FormTemplateViewModel?> GetFormTemplateByIdAsync(int id);
    Task<bool> CreateFormTemplateAsync(FormTemplateFormViewModel model);
    Task<bool> UpdateFormTemplateAsync(int id, FormTemplateFormViewModel model);
    Task<bool> DeleteFormTemplateAsync(int id);

    // â”€â”€â”€ Extracted Fields â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<IEnumerable<ExtractedFieldViewModel>> GetExtractedFieldsAsync(int pdfId);
    Task<bool> UpdateExtractedFieldAsync(int pdfId, int fieldId, string? correctedValue, string? notes);

    // â”€â”€â”€ Audit Log â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<IEnumerable<AuditLogViewModel>> GetAuditLogsAsync(int pdfId);

    // â”€â”€â”€ Preview Images â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<Stream?> GetHighlightedImageStreamAsync(int pdfId, int pageNo);
    Task<Stream?> GetCleanImageStreamAsync(int pdfId, int pageNo);

    // â”€â”€â”€ XML â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<XmlArchiveViewModel?> GetXmlArchiveAsync(int pdfId);
    Task<bool> CreateXmlAsync(int pdfId);
    Task<Stream?> GetXmlDownloadStreamAsync(int pdfId);

    // â”€â”€â”€ Integration â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<IntegrationJobViewModel?> GetIntegrationStatusAsync(int pdfId);
    Task<bool> SendToIntegrationAsync(int pdfId);
    Task<bool> RetryIntegrationAsync(int pdfId);

    // â”€â”€â”€ Institution Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<List<InstitutionDto>> GetAllInstitutionsAsync();
    Task<InstitutionDto?> GetInstitutionAsync(int id);
    Task<bool> CreateInstitutionAsync(CreateInstitutionDto dto);
    Task<bool> UpdateInstitutionAsync(int id, UpdateInstitutionDto dto);
    Task<bool> DeleteInstitutionAsync(int id);

    // â”€â”€â”€ User Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<List<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserAsync(int id);
    Task<bool> CreateUserAsync(CreateUserDto dto);
    Task<bool> UpdateUserAsync(int id, UpdateUserDto dto);
    Task<bool> DeleteUserAsync(int id);

    // â”€â”€â”€ Settings Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    Task<SettingsDto?> GetSettingsAsync();
    Task<bool> UpdateSettingsAsync(SettingsDto dto);
}
