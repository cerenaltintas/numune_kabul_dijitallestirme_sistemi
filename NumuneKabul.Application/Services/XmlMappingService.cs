using System.Collections.Generic;
using System.Linq;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.Application.Services;

/// <summary>
/// XML dönüştürme stratejilerini (MockRest, HL7, FHIR) yöneten orkestratör servis.
/// İstek geldiğinde uygun IXmlMapperStrategy'yi seçerek işlemi gerçekleştirir.
/// </summary>
public class XmlMappingService : IXmlMappingService
{
    private readonly IEnumerable<IXmlMapperStrategy> _strategies;

    public XmlMappingService(IEnumerable<IXmlMapperStrategy> strategies)
    {
        _strategies = strategies;
    }

    public string MapToTargetFormat(string internalXml, string targetFormat)
    {
        if (string.IsNullOrWhiteSpace(internalXml))
            throw new System.ArgumentException("Çevrilecek kaynak XML boş olamaz.", nameof(internalXml));

        // Hedef formata uygun stratejiyi bul (Örn: "MockRest")
        var strategy = _strategies.FirstOrDefault(s => string.Equals(s.TargetFormat, targetFormat, System.StringComparison.OrdinalIgnoreCase));

        if (strategy == null)
            throw new System.NotSupportedException($"'{targetFormat}' hedef formatı için tanımlı bir XML Mapping stratejisi bulunamadı.");

        // Stratejiyi çalıştır ve dönüştürülmüş metni dön
        return strategy.Map(internalXml);
    }
}
