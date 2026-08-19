using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NumuneKabul.Web.Models;

public class PdfUploadViewModel
{
    [Required(ErrorMessage = "Lütfen bir PDF dosyası seçin.")]
    [Display(Name = "PDF Dosyası")]
    public IFormFile File { get; set; } = null!;

    [Display(Name = "Kurum")]
    public int? InstitutionId { get; set; }

    [Display(Name = "Şablon (Opsiyonel)")]
    public int? TemplateId { get; set; }
}
