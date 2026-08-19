using System.ComponentModel.DataAnnotations;

namespace NumuneKabul.Application.DTOs;

public class CreateInstitutionDto
{
    [Required(ErrorMessage = "Kurum adý zorunludur.")]
    [StringLength(200, ErrorMessage = "Kurum adý en fazla 200 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;
}
