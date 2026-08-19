namespace NumuneKabul.Domain.Enums;

/// <summary>
/// IntegrationJob'un mock LIS/HBYS servisiyle entegrasyon durumunu temsil eder.
/// Magic string kullanımını önlemek için IntegrationService'de kullanılır.
/// </summary>
public enum IntegrationStatus
{
    Sending,  // Gönderim başlatıldı
    Sent,     // Başarıyla gönderildi
    Failed,   // Gönderim başarısız
    Retrying  // Yeniden gönderim deneniyor
}
