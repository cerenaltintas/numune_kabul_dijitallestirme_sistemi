namespace NumuneKabul.Application.DTOs;

/// <summary>
/// Kullanıcının OCR sonucunu manuel düzelterek kaydetmesi için kullanılan DTO.
/// </summary>
public class UpdateExtractedFieldDto
{
    public int Id { get; set; }
    public string? CorrectedValue { get; set; }
    public string? Notes { get; set; }
}
