using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Application.DTOs;
using Tesseract;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace NumuneKabul.Infrastructure.Services;

/// <summary>
/// Tesseract OCR servisi — Zonal (Şablon Bazlı) OCR implementasyonu.

/// </summary>
public class TesseractOcrService : IOcrService
{

    // Tesseract kütüphanesi thread-safe değildir ve aynı anda fazla işlem CPU/RAM'i tüketir.
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(4, 4);

    private readonly ILogger<TesseractOcrService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOcrTemplateProvider _templateProvider;

    public TesseractOcrService(ILogger<TesseractOcrService> logger, IConfiguration configuration, IOcrTemplateProvider templateProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _templateProvider = templateProvider;
    }

    public async Task<OcrEngineResultDto> ExtractTextFromImagesAsync(List<string> imagePaths, int? templateId = null)
    {
        _logger.LogInformation("Tesseract OCR motoru başlatıldı. İşlenecek resim sayısı: {Count}, Şablon ID: {TemplateId}", imagePaths.Count, templateId);

        // Limit concurrent OCR operations
        await _semaphore.WaitAsync();
        try
        {
            return await Task.Run(() =>
            {
                var corrections = _configuration
                .GetSection("OcrCorrections")
                .Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();

            string tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
            var combinedText = new StringBuilder();
            var allWords = new List<OcrWordDto>();

            using var engine = new TesseractEngine(tessDataPath, "tur", EngineMode.Default);
            engine.SetVariable("preserve_interword_spaces", "1");
            engine.SetVariable("tessedit_char_blacklist", "|~_{}[]<>\\");

            var totalConfidence = 0m;
            int processedCount = 0;
            
            // Şablonu provider üzerinden alıyoruz.
            var template = templateId.HasValue 
                ? _templateProvider.GetTemplateById(templateId.Value) 
                : _templateProvider.GetDefaultTemplate();

            foreach (var imagePath in imagePaths)
            {
                if (!File.Exists(imagePath)) continue;

                _logger.LogInformation("Sayfa işleniyor: {FileName}", Path.GetFileName(imagePath));

                var displayName = GetDisplayFileName(imagePath);
                
                combinedText.AppendLine($"--- {displayName} ---");

                // Sayfa metninin (pageText) combinedText içindeki tam başlama noktasını (başlık eklendikten sonraki halini) yakalıyoruz.
                int contentStartOffset = combinedText.Length;

                // HER ZAMAN TAM SAYFA OCR (Full Page OCR) YAP (Spatial/Keyword Stratejisi için gereklidir)
                var fullResult = ExtractFullPageText(engine, imagePath, processedCount + 1, corrections);
                
                string pageText = fullResult.text;
                decimal confidence = fullResult.confidence;
                List<OcrWordDto> pageWords = fullResult.words;

                // Eğer Şablon (Zone'lar) varsa, AYRICA Zonal OCR yapıp metnin sonuna ekle (Zonal Strateji için)
                if (template != null && template.Zones.Any())
                {
                    var zonalResult = ExtractZonalText(engine, imagePath, template, processedCount + 1, corrections);
                    string zonalSeparator = "\n\n--- ZONAL AREAS ---\n";
                    int zonalOffset = pageText.Length + zonalSeparator.Length;
                    
                    pageText += zonalSeparator + zonalResult.text;
                    
                    foreach(var w in zonalResult.words)
                    {
                        w.StartIndex += zonalOffset;
                        pageWords.Add(w);
                    }
                }
                
                combinedText.AppendLine(pageText);
                combinedText.AppendLine();
                
                // Alt metotlar kendi lokal indekslerini döndürdü. 
                // Orchestrator olarak burada global offset'i ekliyoruz.
                foreach (var word in pageWords)
                {
                    word.StartIndex += contentStartOffset;
                }
                
                allWords.AddRange(pageWords);
                totalConfidence += confidence;
                processedCount++;
            }

            _logger.LogInformation("Tesseract OCR işlemi başarıyla tamamlandı.");
            
            var avgConf = processedCount > 0 ? totalConfidence / processedCount : 0m;
            
            return new OcrEngineResultDto 
            {
                Text = combinedText.ToString(),
                AverageConfidence = avgConf,
                Words = allWords
            };
        });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // ─── Private Zonal Extraction Method

    private (string text, decimal confidence, List<OcrWordDto> words) ExtractZonalText(
        TesseractEngine engine, 
        string imagePath, 
        OcrTemplateDto template, 
        int pageNo,
        Dictionary<string, string> corrections)
    {
        using var img = Pix.LoadFromFile(imagePath);
        using var grayImg = img.ConvertRGBToGray();
        // ZONAL OCR'da Deskew YAPILMAMALIDIR! 
        // Çünkü Deskew işlemi resmi döndürür ve piksellerin yerini değiştirir.
        // Kullanıcının şablon üzerinden (orijinal resimden) çizdiği X, Y, W, H koordinatları,
        // deskew edilmiş resimde tamamen yanlış yerlere denk gelir.
        var targetImg = grayImg;

        var sb = new StringBuilder();
        var words = new List<OcrWordDto>();
        decimal totalConfidence = 0;
        int zoneCount = 0;

        foreach (var zone in template.Zones)
        {
            // PSM Ayarı (Varsayılan olarak 7 - Single Line)
            var psm = zone.Psm ?? (int)PageSegMode.SingleLine;
            // Akıllı PSM düzeltmesi: Eğer bölge çok yüksekse (çok satırlı bir alansa) ve SingleLine seçilmişse bunu SingleBlock yap.
            if (psm == (int)PageSegMode.SingleLine && zone.Height > 100)
            {
                psm = (int)PageSegMode.SingleBlock;
                _logger.LogInformation("Bölge '{Key}' için çok satırlı metin algılandı (Height: {Height}). PSM {OldPsm} yerine {NewPsm} (SingleBlock) kullanılacak.", zone.Key, zone.Height, (int)PageSegMode.SingleLine, psm);
            }
            engine.DefaultPageSegMode = (PageSegMode)psm;

            int x = zone.X;
            int y = zone.Y;
            int w = zone.Width;
            int h = zone.Height;

            if (template.BaseImageWidth.HasValue && template.BaseImageWidth.Value > 0 && 
                template.BaseImageHeight.HasValue && template.BaseImageHeight.Value > 0)
            {
                float scaleX = (float)targetImg.Width / template.BaseImageWidth.Value;
                float scaleY = (float)targetImg.Height / template.BaseImageHeight.Value;

                x = (int)Math.Round(x * scaleX);
                y = (int)Math.Round(y * scaleY);
                w = (int)Math.Round(w * scaleX);
                h = (int)Math.Round(h * scaleY);
            }

            // Tesseract'ın çok dar çizilmiş (tight) kutuları okuyamaması sorununu çözmek için
            // kutu etrafına 5-10 piksel boşluk (padding) ekliyoruz.
            int padding = 10;
            x = Math.Max(0, x - padding);
            y = Math.Max(0, y - padding);
            
            // Genişlik ve yüksekliği de iki taraflı padding kadar artır
            // Ancak resim boyutlarını aşmamasına dikkat et
            w = Math.Min(targetImg.Width - x, w + (padding * 2));
            h = Math.Min(targetImg.Height - y, h + (padding * 2));

            var rect = new Rect(x, y, w, h);
            
            // Eğer koordinatlar resim sınırlarını aşıyorsa güvenli hale getir
            if (rect.X1 < 0 || rect.Y1 < 0 || rect.X2 > targetImg.Width || rect.Y2 > targetImg.Height)
            {
                _logger.LogWarning("Bölge sınırları resim boyutunu aşıyor. Key: {Key}", zone.Key);
                continue;
            }

            using var page = engine.Process(targetImg, rect);
            
            // Güven skoru
            totalConfidence += (decimal)page.GetMeanConfidence();
            zoneCount++;

            string linePrefix = $"{zone.Key}: ";
            sb.Append(linePrefix);
            
            bool hasWords = false;
            using var iter = page.GetIterator();
            iter.Begin();
            do
            {
                var wordText = iter.GetText(PageIteratorLevel.Word);
                if (string.IsNullOrWhiteSpace(wordText)) continue;
                
                var correctedWord = ApplyOcrCorrections(wordText, corrections).Trim();
                if (string.IsNullOrWhiteSpace(correctedWord)) continue;
                
                hasWords = true;
                int startIndex = sb.Length;
                sb.Append(correctedWord);
                
                if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out Rect bounds))
                {
                    var wordConfidence = iter.GetConfidence(PageIteratorLevel.Word) / 100f;
                    words.Add(new OcrWordDto
                    {
                        Text = correctedWord,
                        X = (int)bounds.X1,
                        Y = (int)bounds.Y1,
                        Width = (int)bounds.Width,
                        Height = (int)bounds.Height,
                        PageNo = pageNo,
                        StartIndex = startIndex,
                        Confidence = (decimal)wordConfidence
                    });
                }
                
                sb.Append(" ");
            } while (iter.Next(PageIteratorLevel.Word));
            
            if (!hasWords)
            {
                // Boş da olsa key'i gösterelim ki formda yeri belli olsun
                sb.Append("[Okunamadı veya Boş]");
            }
            
            sb.AppendLine();
        }

        decimal avgConfidence = zoneCount > 0 ? totalConfidence / zoneCount : 0;
        return (sb.ToString(), avgConfidence, words);
    }

    // ─── Private Full Page Fallback Method

    private (string text, decimal confidence, List<OcrWordDto> words) ExtractFullPageText(
        TesseractEngine engine, 
        string imagePath, 
        int pageNo,
        Dictionary<string, string> corrections)
    {
        engine.DefaultPageSegMode = PageSegMode.Auto; // SparseText yerine Auto daha güvenli form okumak için (Şablon yoksa)
        
        using var img = Pix.LoadFromFile(imagePath);
        using var grayImg = img.ConvertRGBToGray();
        // ZONAL OCR'da olduğu gibi tam sayfa OCR'da da Deskew YAPILMAMALIDIR!
        // Çünkü çıkartılan kelimelerin (OcrWordDto) koordinatları arayüzde orjinal resim üzerine çizilecektir.
        var targetImg = grayImg;
        
        using var page = engine.Process(targetImg);
        var confidence = (decimal)page.GetMeanConfidence();
        var words = new List<OcrWordDto>();
        var sb = new StringBuilder();
        
        using var iter = page.GetIterator();
        iter.Begin();
        
        do
        {
            var wordText = iter.GetText(PageIteratorLevel.Word);
            if (string.IsNullOrWhiteSpace(wordText)) continue;
            
            var correctedWord = ApplyOcrCorrections(wordText, corrections);
            
            int startIndex = sb.Length;
            sb.Append(correctedWord);

            if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out Rect bounds))
            {
                var wordConfidence = iter.GetConfidence(PageIteratorLevel.Word) / 100f;
                words.Add(new OcrWordDto
                {
                    Text = correctedWord,
                    X = (int)bounds.X1,
                    Y = (int)bounds.Y1,
                    Width = (int)bounds.Width,
                    Height = (int)bounds.Height,
                    PageNo = pageNo,
                    StartIndex = startIndex,
                    Confidence = (decimal)wordConfidence
                });
            }

            if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
            {
                sb.AppendLine();
            }
            else
            {
                sb.Append(" ");
            }
            
        } while (iter.Next(PageIteratorLevel.Word));
        
        return (sb.ToString(), confidence, words);
    }

    // ─── Yardımcı Metodlar

    private static string ApplyOcrCorrections(string text, Dictionary<string, string> corrections)
    {
        foreach (var kvp in corrections)
        {
            text = text.Replace(kvp.Key, kvp.Value);
        }
        return text;
    }

    private static string GetDisplayFileName(string imagePath)
    {
        var name = Path.GetFileName(imagePath);
        if (name.Contains("_sayfa"))
        {
            var idx = name.IndexOf("_sayfa", StringComparison.Ordinal);
            name = name.Substring(idx + 1);
        }
        return name;
    }
}
