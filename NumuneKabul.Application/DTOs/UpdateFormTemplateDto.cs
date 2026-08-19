namespace NumuneKabul.Application.DTOs;

//Arayüzden var olan bir şablonu düzenlerken sunucuya yollanan kutu
public class UpdateFormTemplateDto
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int? BaseImageWidth { get; set; }
    public int? BaseImageHeight { get; set; }

    public List<UpdateTemplateFieldDto> TemplateFields { get; set; } = new();
}

public class UpdateTemplateFieldDto
{
    public int Id { get; set; }
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
