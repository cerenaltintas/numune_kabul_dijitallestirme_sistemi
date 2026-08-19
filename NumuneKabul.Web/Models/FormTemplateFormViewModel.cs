using System.ComponentModel.DataAnnotations;

namespace NumuneKabul.Web.Models;

public class FormTemplateFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Kurum seçimi zorunludur.")]
    [Display(Name = "Kurum")]
    public int InstitutionId { get; set; }

    [Required(ErrorMessage = "Şablon adı zorunludur.")]
    [Display(Name = "Şablon Adı")]
    [MaxLength(100, ErrorMessage = "Şablon adı en fazla 100 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Description { get; set; }

    [Display(Name = "Aktif Mi?")]
    public bool IsActive { get; set; } = true;

    // Zonal OCR Referans Çözünürlüğü
    public int? BaseImageWidth { get; set; }
    public int? BaseImageHeight { get; set; }

    // View için listelemede kullanılacak Kurumlar
    public IEnumerable<InstitutionViewModel>? Institutions { get; set; }

    // Alanlar
    public List<TemplateFieldFormViewModel> TemplateFields { get; set; } = new();
}

public class TemplateFieldFormViewModel
{
    public int? Id { get; set; }
    
    [Required(ErrorMessage = "Alan adı zorunludur.")]
    public string FieldName { get; set; } = string.Empty;
    
    public string? Regex { get; set; }
    
    public bool Required { get; set; }
    
    public string DataType { get; set; } = "string";
    
    public int OrderNo { get; set; }

    public int? X { get; set; }
    public int? Y { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Psm { get; set; }
}
