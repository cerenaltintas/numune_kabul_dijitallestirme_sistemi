using NumuneKabul.Application.DTOs;

namespace NumuneKabul.Application.Interfaces;

/// <summary>
/// Zonal OCR için kullanılacak şablonları sağlamakla yükümlü arayüz.
/// Şablonları sağlayan servis. Şablonlar veritabanından veya yapılandırma dosyasından gelebilir.
/// </summary>
public interface IOcrTemplateProvider
{
    /// <summary>
    /// Verilen form tipine/ismine göre OCR şablonunu döndürür.
    /// </summary>
    /// <param name="templateName">İstenen şablonun adı (Örn: "Bilkent_Patoloji_Formu")</param>
    /// <returns>Bulunan şablon veya null</returns>
    OcrTemplateDto? GetTemplate(string templateName);
    
    /// <summary>
    /// Verilen ID'ye göre OCR şablonunu döndürür.
    /// </summary>
    /// <param name="id">Şablon ID'si</param>
    /// <returns>Bulunan şablon veya null</returns>
    OcrTemplateDto? GetTemplateById(int id);

    /// <summary>
    /// Sistemde kayıtlı varsayılan şablonu getirir. (Sadece tek tip form işleniyorsa veya şablon seçilmemişse kullanılır)
    /// </summary>
    OcrTemplateDto? GetDefaultTemplate();
}
