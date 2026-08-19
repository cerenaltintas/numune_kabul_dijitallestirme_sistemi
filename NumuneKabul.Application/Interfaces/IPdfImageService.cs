namespace NumuneKabul.Application.Interfaces;

/// <summary>
/// Yüklenen PDF belgelerini analiz ve önizleme amacıyla yüksek çözünürlüklü resimlere (PNG/JPG) dönüştüren servis sözleşmesi.
/// </summary>
public interface IPdfImageService
{
    /// <summary>
    /// PDF dosyasını sayfa sayfa resimlere dönüştürür.
    /// </summary>
    /// <param name="pdfFilePath">Okunacak PDF dosyasının bilgisayardaki tam yolu</param>
    /// <param name="outputDirectory">Resimlerin kaydedileceği hedef klasör yolu</param>
    /// <returns>Oluşturulan resim dosyalarının yollarını liste olarak döner</returns>
    Task<List<string>> ConvertToImagesAsync(string pdfFilePath, string outputDirectory);

    /// <summary>
    /// Verilen PDF ve sayfa numarasına ait oluşturulmuş resim dosyasının fiziksel tam yolunu döner.
    /// </summary>
    string GetImageFilePath(int pdfId, int pageNo);
}
