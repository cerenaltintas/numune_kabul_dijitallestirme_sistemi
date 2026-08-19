namespace NumuneKabul.Application.Interfaces;
//Resimlerin içindeki yazıları okuyup metne dönüştüren OCR motoru sözleşmesi
public interface IOcrService
{
    /// <summary>
    /// Verilen resim listesinin içindeki yazıları okur ve metne çevirir.
    /// </summary>
    /// <param name="imagePaths">Okunacak resimlerin yolları (Listesi)</param>
    /// <param name="templateId">Kullanılacak OCR şablonunun ID'si (Opsiyonel)</param>
    /// <returns>Resimlerden okunan tüm metinler ve genel güven skoru</returns>
    Task<DTOs.OcrEngineResultDto> ExtractTextFromImagesAsync(List<string> imagePaths, int? templateId = null);
}
