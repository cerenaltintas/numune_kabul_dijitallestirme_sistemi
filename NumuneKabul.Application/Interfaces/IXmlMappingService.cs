namespace NumuneKabul.Application.Interfaces;

/// <summary>
/// Tüm XML dönüştürme (mapping) stratejilerini yöneten servis (Orchestrator).
/// İstenen formata (TargetFormat) göre ilgili IXmlMapperStrategy'yi bularak işlemi delege eder.
/// </summary>
public interface IXmlMappingService
{
    /// <summary>
    /// İç (standart) XML metnini alır ve hedeflenen dış formata dönüştürür.
    /// </summary>
    /// <param name="internalXml">Veritabanından alınan standart XML</param>
    /// <param name="targetFormat">Hedef format (örn: "MockRest", "HL7")</param>
    /// <returns>Hedef sistemin beklediği yapıdaki XML metni</returns>
    string MapToTargetFormat(string internalXml, string targetFormat);
}
