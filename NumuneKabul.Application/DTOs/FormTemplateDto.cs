namespace NumuneKabul.Application.DTOs;

public class FormTemplateDto
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int? BaseImageWidth { get; set; }
    public int? BaseImageHeight { get; set; }
    public List<TemplateFieldDto> TemplateFields { get; set; } = new();
}
