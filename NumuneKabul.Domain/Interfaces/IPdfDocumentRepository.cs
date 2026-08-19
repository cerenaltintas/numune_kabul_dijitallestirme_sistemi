using NumuneKabul.Domain.Entities;

namespace NumuneKabul.Domain.Interfaces;

public interface IPdfDocumentRepository : IGenericRepository<PdfDocument>
{
    Task<IEnumerable<PdfDocument>> GetByInstitutionIdAsync(int institutionId);
    Task<IEnumerable<PdfDocument>> GetByStatusAsync(string status);
    Task<(IEnumerable<PdfDocument> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize);
    Task<(IEnumerable<PdfDocument> Items, int TotalCount)> GetPaginatedByInstitutionAsync(int page, int pageSize, int institutionId);
}
