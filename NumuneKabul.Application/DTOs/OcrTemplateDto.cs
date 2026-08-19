namespace NumuneKabul.Application.DTOs;

/// <summary>
/// Zonal OCR için kullanılacak şablonun genel tanımını tutar.
/// Şablon verisini taşır.
/// İçerisinde TemplateFieldDto listesi barındırır.
/// </summary>
public class OcrTemplateDto
{
    public string TemplateName { get; set; } = string.Empty;
    public int? BaseImageWidth { get; set; }
    public int? BaseImageHeight { get; set; }
    public List<OcrZoneDto> Zones { get; set; } = new();
}

/// <summary>
/// Zonal OCR işleminde okunacak belirli bir dikdörtgen alanı (Zone) temsil eder.
/// </summary>
public class OcrZoneDto
{
    /// <summary>Çıkarılacak alanın anahtar ismi (örn: TCKimlik)</summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>Dikdörtgenin X başlangıç noktası (Sol üst köşenin X koordinatı)</summary>
    public int X { get; set; }
    
    /// <summary>Dikdörtgenin Y başlangıç noktası (Sol üst köşenin Y koordinatı)</summary>
    public int Y { get; set; }
    
    /// <summary>Dikdörtgenin Genişliği</summary>
    public int Width { get; set; }
    
    /// <summary>Dikdörtgenin Yüksekliği</summary>
    public int Height { get; set; }
    
    /// <summary>Tesseract Page Segmentation Mode (PSM). Sabit alanlar için genelde 7 (Single Line) veya 6 (Single Block) kullanılır.</summary>
    public int? Psm { get; set; }
}
