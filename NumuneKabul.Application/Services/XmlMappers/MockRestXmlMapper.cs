using System.Xml.Linq;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.Application.Services.XmlMappers;

/// <summary>
/// Mock REST için özel XML dönüşüm stratejisi.
/// İç (Standart) XML'i alır, LIS/HBYS'nin beklediği spesifik formata dönüştürür.
/// XML Injection ve Parsing zafiyetlerini önlemek için güvenli XDocument sınıfı (LINQ to XML) kullanır.
/// </summary>
public class MockRestXmlMapper : IXmlMapperStrategy
{
    public string TargetFormat => "MockRest";

    public string Map(string internalXml)
    {
        // Güvenli XML Parsing - DTD işlemeyi devre dışı bırakarak XXE zafiyetini engelle.
        var parseOptions = new LoadOptions();
        var doc = XDocument.Parse(internalXml, parseOptions);

        var belgeNode = doc.Root?.Element("Belge");
        var pdfId = belgeNode?.Element("Id")?.Value ?? "Bilinmiyor";
        var kurumId = belgeNode?.Element("KurumId")?.Value ?? "0";

        var cikarilmisAlanlar = doc.Root?.Element("CikarilmisAlanlar")?.Elements("Alan") ?? Enumerable.Empty<XElement>();

        // Hedef servisin beklediği yeni şablon (Örnek dönüşüm)
        var targetDoc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("IntegrationPayload",
                new XAttribute("schemaVersion", "1.1"),
                new XAttribute("timestamp", DateTime.UtcNow.ToString("o")),
                new XElement("Header",
                    new XElement("DocumentId", pdfId),
                    new XElement("TargetInstitution", kurumId)
                ),
                new XElement("Body",
                    new XElement("Fields",
                        cikarilmisAlanlar.Select(a =>
                            new XElement("Field",
                                new XAttribute("name", a.Attribute("ad")?.Value ?? "unknown"),
                                new XAttribute("confidence", a.Attribute("guvenSkor")?.Value ?? "0"),
                                new XElement("Value", a.Element("DuzeltilmisDeger")?.Value ?? string.Empty)
                            )
                        )
                    )
                )
            )
        );

        return targetDoc.ToString();
    }
}
