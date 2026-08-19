namespace NumuneKabul.Web.Models;

public class InstitutionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class FormTemplateViewModel
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<TemplateFieldViewModel> TemplateFields { get; set; } = new();
}

public class TemplateFieldViewModel
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? Regex { get; set; }
    public bool Required { get; set; }
    public string DataType { get; set; } = "string";
    public int OrderNo { get; set; }
    
    // Zonal OCR Coordinates
    public int? X { get; set; }
    public int? Y { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Psm { get; set; }
}
