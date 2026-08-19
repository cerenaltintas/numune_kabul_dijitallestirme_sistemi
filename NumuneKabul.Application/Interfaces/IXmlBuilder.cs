using NumuneKabul.Domain.Entities;

namespace NumuneKabul.Application.Interfaces;

/// <summary>
/// XML üretim sürecini soyutlayan Builder arayüzü.
/// Farklı XML formatları (Standart, HL7 vb.) oluşturmak için
/// Fluent Builder arayüzü. uygulayan sınıflar tarafından üretilebilir.
/// </summary>
public interface IXmlBuilder
{
    IXmlBuilder StartDocument(string version, string date);
    IXmlBuilder AddDocumentInfo(PdfDocument pdf);
    IXmlBuilder AddOcrText(string ocrText);
    IXmlBuilder AddExtractedFields(IEnumerable<ExtractedField> fields);
    string Build();
}
