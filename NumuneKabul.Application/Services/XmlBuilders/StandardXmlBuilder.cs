using System.Text;
using System.Xml;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Domain.Entities;

namespace NumuneKabul.Application.Services.XmlBuilders;

/// <summary>
/// Standart XML yapısını üreten sınıftır.
/// </summary>
public class StandardXmlBuilder : IXmlBuilder, IDisposable
{
    private readonly StringBuilder _sb;
    private readonly XmlWriter _writer;

    public StandardXmlBuilder()
    {
        _sb = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };

        _writer = XmlWriter.Create(_sb, settings);
    }

    public IXmlBuilder StartDocument(string version, string date)
    {
        _writer.WriteStartDocument();
        _writer.WriteStartElement("NumuneKabulBelgesi");
        _writer.WriteAttributeString("versiyon", version);
        _writer.WriteAttributeString("uretimTarihi", date);
        return this;
    }

    public IXmlBuilder AddDocumentInfo(PdfDocument pdf)
    {
        if (pdf == null) throw new ArgumentNullException(nameof(pdf));

        _writer.WriteStartElement("Belge");
        _writer.WriteElementString("Id", pdf.Id.ToString());
        _writer.WriteElementString("DosyaAdi", pdf.FileName);
        _writer.WriteElementString("KurumId", pdf.InstitutionId.ToString());
        _writer.WriteElementString("YuklenmeTarihi", pdf.UploadDate.ToString("o"));
        _writer.WriteElementString("Durum", pdf.Status);
        _writer.WriteEndElement(); // Belge
        return this;
    }

    public IXmlBuilder AddOcrText(string ocrText)
    {
        _writer.WriteStartElement("OcrMetni");
        _writer.WriteCData(ocrText ?? string.Empty);
        _writer.WriteEndElement(); // OcrMetni
        return this;
    }

    public IXmlBuilder AddExtractedFields(IEnumerable<ExtractedField> fields)
    {
        if (fields == null) return this;

        _writer.WriteStartElement("CikarilmisAlanlar");
        foreach (var field in fields.OrderBy(f => f.FieldName))
        {
            _writer.WriteStartElement("Alan");
            _writer.WriteAttributeString("ad", field.FieldName);
            _writer.WriteAttributeString("guvenSkor", field.Confidence.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            _writer.WriteAttributeString("durum", field.Status);
            _writer.WriteAttributeString("sayfaNo", field.PageNo.ToString());

            _writer.WriteElementString("HamDeger", field.RawValue ?? string.Empty);
            _writer.WriteElementString("DuzeltilmisDeger", field.CorrectedValue ?? field.RawValue ?? string.Empty);

            // Koordinatlar
            _writer.WriteStartElement("Koordinatlar");
            _writer.WriteAttributeString("x", field.X.ToString());
            _writer.WriteAttributeString("y", field.Y.ToString());
            _writer.WriteAttributeString("genislik", field.Width.ToString());
            _writer.WriteAttributeString("yukseklik", field.Height.ToString());
            _writer.WriteEndElement(); // Koordinatlar

            _writer.WriteEndElement(); // Alan
        }
        _writer.WriteEndElement(); // CikarilmisAlanlar
        return this;
    }

    public string Build()
    {
        _writer.WriteEndElement(); // NumuneKabulBelgesi
        _writer.WriteEndDocument();
        _writer.Flush();
        return _sb.ToString();
    }

    public void Dispose()
    {
        _writer?.Dispose();
    }
}
