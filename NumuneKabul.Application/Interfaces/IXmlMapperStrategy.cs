namespace NumuneKabul.Application.Interfaces;

/// <summary>
/// XML dönüştürme stratejileri için ortak arayüz.
/// Farklı dış sistemlere (HL7, FHIR, MockRest) dönüştürme işlemleri bu arayüzden türer.
/// XmlService bu arayüzü kullanarak hedef formatı üretir.
/// </summary>
public interface IXmlMapperStrategy
{
    /// <summary>
    /// Bu stratejinin hangi hedef formata ait olduğunu belirtir (Örn: "MockRest", "HL7").
    /// </summary>
    string TargetFormat { get; }

    /// <summary>
    /// Sistemde üretilen standart XML'i alır, hedeflenen dış sistemin XML formatına dönüştürür.
    /// </summary>
    /// <param name="internalXml">Sistem içi standart XML</param>
    /// <returns>Dış sistem için dönüştürülmüş (mapped) XML</returns>
    string Map(string internalXml);
}
