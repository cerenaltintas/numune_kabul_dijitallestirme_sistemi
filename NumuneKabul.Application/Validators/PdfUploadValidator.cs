using FluentValidation;

namespace NumuneKabul.Application.Validators;

//Zararlı ve formatı bozuk dosya yüklemelerini engellemek için kullanılan doğrulayıcı sınıf.
public class PdfUploadValidator : AbstractValidator<(Stream FileStream, string FileName)>
{
    public PdfUploadValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("Dosya boş olamaz.")
            .Must(s => s != null && s.Length > 0).WithMessage("Dosya içeriği boş olamaz.")
            .Must(s => s != null && s.Length <= 50 * 1024 * 1024).WithMessage("Dosya boyutu 50MB'ı aşamaz.")
            .Must(IsRealPdf).WithMessage("Yüklenen dosyanın içeriği geçerli bir PDF değil! (Zararlı yazılım şüphesi)");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("Dosya adı boş olamaz.")
            .Must(f => !string.IsNullOrEmpty(f) && f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sadece PDF dosyaları yüklenebilir.");
    }

    private bool IsRealPdf(Stream stream)
    {
        if (stream == null || stream.Length < 4) return false;

        var headerBytes = new byte[4];
        
        var initialPosition = stream.Position;
        stream.Position = 0;
        
        stream.Read(headerBytes, 0, 4);
        stream.Position = initialPosition;

        return headerBytes[0] == 0x25 && 
               headerBytes[1] == 0x50 && 
               headerBytes[2] == 0x44 && 
               headerBytes[3] == 0x46;
    }
}
