namespace NumuneKabul.Domain.Entities;

public class FormTemplate //kurum şablonu
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Institution Institution { get; set; } = null!;
    public ICollection<TemplateField> TemplateFields { get; set; } = new List<TemplateField>();

    // Zonal OCR Referans Çözünürlüğü
    public int? BaseImageWidth { get; set; }
    public int? BaseImageHeight { get; set; }
}
