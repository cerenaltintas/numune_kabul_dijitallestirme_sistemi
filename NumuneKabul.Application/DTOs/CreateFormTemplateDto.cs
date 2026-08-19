namespace NumuneKabul.Application.DTOs;

public class CreateFormTemplateDto
{
    public int InstitutionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int? BaseImageWidth { get; set; }
    public int? BaseImageHeight { get; set; }

    public List<CreateTemplateFieldDto> TemplateFields { get; set; } = new();
}

public class CreateTemplateFieldDto
{
    public string FieldName { get; set; } = string.Empty;
    public string? Regex { get; set; }
    public string? Keyword { get; set; }
    public string? ValidationRegex { get; set; }
    public bool Required { get; set; }
    public string DataType { get; set; } = "string";
    public int OrderNo { get; set; }

    public int? X { get; set; }
    public int? Y { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Psm { get; set; }
}
