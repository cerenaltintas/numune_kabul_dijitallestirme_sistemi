namespace NumuneKabul.Domain.Entities;

public class TemplateField //alan şablonu
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? Regex { get; set; }
    public string? Keyword { get; set; } // Added Keyword back
    public string? ValidationRegex { get; set; }
    public bool Required { get; set; }
    public string DataType { get; set; } = "string";
    public int OrderNo { get; set; }

    // Zonal OCR Coordinates
    public int? X { get; set; }
    public int? Y { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Psm { get; set; } = 7;

    public FormTemplate FormTemplate { get; set; } = null!;
}
