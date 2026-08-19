namespace NumuneKabul.Domain.Enums;

public enum DocumentStatus // belgenin sistemdeki güncel aşaması
{
    Uploaded,
    OcrProcessing,
    OcrCompleted,
    FieldsExtracted,
    Corrected,        // Manuel düzeltme yapıldı
    Validated,
    XmlCreated,       // XML üretildi ve arşivlendi
    XmlGenerated,
    IntegrationSent,  // Mock servise başarıyla gönderildi
    IntegrationFailed,// Mock servise gönderim başarısız
    Sent,
    NeedsManualReview,
    Error
}
