namespace NumuneKabul.Application.DTOs;

// Pdf yüklenirken arayüze sonuç döndürmek için kullanılır
public class PdfUploadResultDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
