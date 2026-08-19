using System.Text.RegularExpressions;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.Infrastructure.Services;

public class FieldValidatorService : IFieldValidatorService
{
    public void ValidateAndAdjustConfidence(ExtractedResultDto result, TemplateFieldDto field)
    {
        if (string.IsNullOrWhiteSpace(result.RawValue))
        {
            return; // Boş ise veya okunamadıysa zaten güven skoru muhtemelen düşüktür veya null'dır.
        }

        if (!string.IsNullOrWhiteSpace(field.ValidationRegex))
        {
            // Validasyon Regex'ine uymuyorsa OCR hatalı karakter okumuş demektir
            if (!Regex.IsMatch(result.RawValue, field.ValidationRegex))
            {
                // Güven skorunu 0.5'e (Warning/İnceleme Gerektirir) düşür
                result.Confidence = Math.Min(result.Confidence, 0.5m);
            }
        }
        else
        {
            // Varsayılan kural: Eğer özel karakterler barındırıyorsa ama sayısal/harf bekleniyorsa
            // DataType'a göre varsayılan validasyon eklenebilir.
            if (field.DataType.Equals("number", StringComparison.OrdinalIgnoreCase))
            {
                if (!Regex.IsMatch(result.RawValue, @"^\d+$"))
                {
                    result.Confidence = Math.Min(result.Confidence, 0.5m);
                }
            }
            else if (field.DataType.Equals("date", StringComparison.OrdinalIgnoreCase))
            {
                // Basit tarih formatı kontrolü
                if (!Regex.IsMatch(result.RawValue, @"^\d{1,2}[./-]\d{1,2}[./-]\d{2,4}$"))
                {
                    result.Confidence = Math.Min(result.Confidence, 0.5m);
                }
            }
        }
    }

    public bool IsValid(ExtractedResultDto result, TemplateFieldDto field)
    {
        if (string.IsNullOrWhiteSpace(result.RawValue)) return false;

        string value = result.RawValue.Trim();

        if (!string.IsNullOrWhiteSpace(field.ValidationRegex))
        {
            // Eğer özel bir regex verilmişse, buna kesinlikle uymalıdır.
            return Regex.IsMatch(value, field.ValidationRegex);
        }

        // Eğer regex verilmemişse DataType üzerinden varsayılan kontroller
        if (field.DataType.Equals("number", StringComparison.OrdinalIgnoreCase))
        {
            // Sadece rakamlardan ve opsiyonel ondalık ayracı oluşmalı
            return Regex.IsMatch(value, @"^[\d.,\s]+$");
        }
        else if (field.DataType.Equals("date", StringComparison.OrdinalIgnoreCase))
        {
            // Basit tarih formatı kontrolü
            return Regex.IsMatch(value, @"\d{1,2}[./-]\d{1,2}[./-]\d{2,4}");
        }
        else
        {
            // DataType "string" veya boş ise
            // Çok fazla çöp karakter barındıran (örn: —— S ge ? aa ., —) 
            // metinleri elemek için basit bir "harf/rakam oranı" veya "en az bir anlamlı harf/rakam" kontrolü.
            bool hasAlphaNumeric = value.Any(char.IsLetterOrDigit);
            if (!hasAlphaNumeric) return false;
            
            return true;
        }
    }
}
