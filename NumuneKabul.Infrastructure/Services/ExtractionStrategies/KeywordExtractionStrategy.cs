using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.Infrastructure.Services.ExtractionStrategies;

/// <summary>
/// Keyword tabanlı alan çıkarma stratejisi.
/// Regex tabanlı tolerans (Fuzzy match) ve Konumsal (Spatial) arama algoritması ile
/// değerleri (sütunları veya satırları) görsel yerleşimine göre (Bounding Box) ayıklar.
/// </summary>
public class KeywordExtractionStrategy : IExtractionStrategy
{
    private readonly ILogger<KeywordExtractionStrategy> _logger;
    private readonly ICoordinateMapperService _coordinateMapper;

    public KeywordExtractionStrategy(ILogger<KeywordExtractionStrategy> logger, ICoordinateMapperService coordinateMapper)
    {
        _logger = logger;
        _coordinateMapper = coordinateMapper;
    }

    public bool CanExecute(TemplateFieldDto field)
    {
        return !string.IsNullOrWhiteSpace(field.Keyword);
    }

    public ExtractedResultDto Extract(OcrEngineResultDto ocrResult, TemplateFieldDto field, List<TemplateFieldDto> allFields)
    {
        var result = new ExtractedResultDto
        {
            FieldName = field.FieldName,
            PageNo = 1,
            Confidence = 0,
            RawValue = null
        };

        try
        {
            if (string.IsNullOrWhiteSpace(field.Keyword)) return result;

            // 1. Keyword'ü tolere edilebilir (boşluklu, O/0 farklılıkları tolere) Regex ile bulalım
            var keywordRegexPattern = BuildFuzzyRegexPattern(field.Keyword);
            
            // Sadece asıl metin üzerinde arama yap. 
            // TesseractOcrService, Zonal okumaları "--- ZONAL AREAS ---" başlığı altında metnin sonuna ekliyor.
            // Bu sahte etiketler (örn: "TC Kimlik No: ") kelime (Word) koordinatlarına sahip olmadığı için
            // eğer Regex onlarla eşleşirse BoundingBox 0,0 döner ve Keyword stratejisi patlar.
            string mainText = ocrResult.Text;
            int zonalSeparatorIndex = ocrResult.Text.IndexOf("--- ZONAL AREAS ---");
            if (zonalSeparatorIndex >= 0)
            {
                mainText = ocrResult.Text.Substring(0, zonalSeparatorIndex);
            }

            var match = Regex.Match(mainText, keywordRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (!match.Success) return result;

            int keywordStartIndex = match.Index;

            // 2. Keyword'ün OCR Words listesindeki BoundingBox'ını (Alanını) bul
            var keywordBox = _coordinateMapper.GetBoundingBoxForMatch(keywordStartIndex, match.Length, ocrResult.Words);

            // Eğer keyword box 0,0 ise OCR kelimeleri (Words) arasında eşleştirilememiştir.
            if (keywordBox.Width == 0 || keywordBox.Height == 0) return result;

            // 3. Değer (Value) Bölgesini Spatial (Konumsal) olarak bul
            // Tablo sütunlarında sağdaki (hemen yandaki) değer hedeflenir.
            
            // Spatial Tolerans: Y ekseninde (Satır hizasında)
            int yTolerance = (int)(keywordBox.Height * 0.8);
            int minX = keywordBox.X + keywordBox.Width; // Sağında
            
            // Tüm sayfadaki kelimelerden bizim satır hizamızda olanları ve sağımızda olanları filtrele
            var valueWords = ocrResult.Words
                .Where(w => w.PageNo == ocrResult.Words[0].PageNo) // Aynı sayfada
                .Where(w => w.Y >= keywordBox.Y - yTolerance && w.Y + w.Height <= keywordBox.Y + keywordBox.Height + yTolerance) // Aynı satır hizasında
                .Where(w => w.X >= minX) // Sağ tarafında
                .OrderBy(w => w.X) // Soldan sağa doğru
                .ToList();

            if (valueWords.Any())
            {
                // Değerleri birleştir, ancak aralarında çok büyük X boşluğu olan yere kadar (Farklı sütun veya alakasız bölgeye taşmayı engelle)
                var sbValue = new System.Text.StringBuilder();
                int currentRight = keywordBox.X + keywordBox.Width;
                
                // Başlangıçta Label ile Value arasında büyük boşluk olabilir (özellikle iki yana yaslı formalarda)
                // Ama ilk kelimeyi bulduktan sonra kelimeler arası boşluk normal olmalı.
                int initialMaxGap = keywordBox.Height * 20; 
                int subsequentMaxGap = keywordBox.Height * 3;
                
                var matchedValueWords = new List<OcrWordDto>();

                foreach (var w in valueWords)
                {
                    // Temizleme: ":" veya "-" gibi işaretleri atla (Eğer değerin ilk karakterleriyse)
                    string cleanText = w.Text.Trim();
                    if (string.IsNullOrWhiteSpace(cleanText)) continue;
                    
                    if (matchedValueWords.Count == 0 && (cleanText == ":" || cleanText == "-" || cleanText == ".")) 
                    {
                        // Sadece : sembolünü atlayıp currentRight'i güncelliyoruz ki boşluk limitine takılmayalım
                        currentRight = w.X + w.Width;
                        continue;
                    }

                    // Dinamik boşluk kontrolü
                    int currentMaxGap = matchedValueWords.Count == 0 ? initialMaxGap : subsequentMaxGap;
                    
                    // Eğer kelimeler arası boşluk çok fazlaysa (başka bir kolonun başlığına geçtiysek), aramayı durdur.
                    if (w.X - currentRight > currentMaxGap) 
                        break;

                    sbValue.Append(w.Text).Append(" ");
                    currentRight = w.X + w.Width;
                    matchedValueWords.Add(w);
                }

                var extractedValue = sbValue.ToString().Trim();
                
                // Başındaki olası yapışık ":" veya "-" işaretlerini temizle
                char[] charsToTrim = { ':', '-', '.', ' ', '—', '_', '>' };
                extractedValue = extractedValue.TrimStart(charsToTrim).Trim();

                if (!string.IsNullOrWhiteSpace(extractedValue))
                {
                    result.RawValue = extractedValue;
                    result.Confidence = matchedValueWords.Any() ? matchedValueWords.Average(w => w.Confidence) : 0.85m;
                    result.PageNo = valueWords.First().PageNo;

                    // Değer kelimelerinin koordinatlarını birleştir
                    int resMinX = matchedValueWords.Min(w => w.X);
                    int resMinY = matchedValueWords.Min(w => w.Y);
                    int resMaxX = matchedValueWords.Max(w => w.X + w.Width);
                    int resMaxY = matchedValueWords.Max(w => w.Y + w.Height);

                    result.X = resMinX;
                    result.Y = resMinY;
                    result.Width = resMaxX - resMinX;
                    result.Height = resMaxY - resMinY;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Keyword (Spatial) arama hatası: {FieldName}", field.FieldName);
        }

        return result;
    }

    /// <summary>
    /// Katı string eşleşmesi yerine toleranslı Regex deseni üretir.
    /// OCR'ın O/0, I/l/1, boşluk veya özel karakter hatalarını tolere eder.
    /// </summary>
    private string BuildFuzzyRegexPattern(string keyword)
    {
        var chars = keyword.ToCharArray();
        var sb = new System.Text.StringBuilder();

        foreach (var c in chars)
        {
            if (char.IsWhiteSpace(c))
            {
                sb.Append(@"\s*");
            }
            else if (char.IsPunctuation(c))
            {
                sb.Append(@"\s*").Append(Regex.Escape(c.ToString())).Append(@"?\s*");
            }
            else
            {
                char upper = char.ToUpperInvariant(c);
                if (upper == 'O' || upper == '0')
                {
                    sb.Append(@"[O0o]");
                }
                else if (upper == 'I' || upper == 'İ' || upper == 'L' || upper == '1')
                {
                    sb.Append(@"[Iİil1]");
                }
                else if (upper == 'C' || upper == 'Ç')
                {
                    sb.Append(@"[CÇcç]");
                }
                else if (upper == 'S' || upper == 'Ş')
                {
                    sb.Append(@"[SŞsş]");
                }
                else if (upper == 'G' || upper == 'Ğ')
                {
                    sb.Append(@"[GĞgğ]");
                }
                else if (upper == 'U' || upper == 'Ü')
                {
                    sb.Append(@"[UÜuü]");
                }
                else if (upper == 'O' || upper == 'Ö')
                {
                    sb.Append(@"[OÖoö]");
                }
                else
                {
                    sb.Append(Regex.Escape(c.ToString()));
                }
            }
        }
        
        return sb.ToString();
    }
}

