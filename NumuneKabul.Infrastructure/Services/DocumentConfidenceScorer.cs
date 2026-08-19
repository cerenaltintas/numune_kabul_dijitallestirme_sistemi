using System;
using System.Collections.Generic;
using System.Linq;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;

namespace NumuneKabul.Infrastructure.Services;

public class DocumentConfidenceScorer : IDocumentConfidenceScorer
{
    // Constants for weighting and bounds
    private const decimal OcrScoreWeight = 0.5m;
    private const decimal FieldsScoreWeight = 0.5m;
    private const decimal MaxScore = 100m;
    private const decimal MinScore = 0m;
    private const decimal DefaultFieldConfidence = 100m;

    public decimal CalculateDocumentScore(decimal ocrConfidence, IEnumerable<ExtractedResultDto> extractedFields, IEnumerable<TemplateFieldDto> templateFields)
    {
        // 1. Check for null inputs
        ArgumentNullException.ThrowIfNull(extractedFields);
        ArgumentNullException.ThrowIfNull(templateFields);

        // 2. Process independent scoring components
        decimal normalizedOcrScore = NormalizeConfidenceScore(ocrConfidence);
        decimal fieldsScore = CalculateRequiredFieldsScore(extractedFields, templateFields);

        // 3. Compute final weighted score
        decimal documentScore = CalculateWeightedFinalScore(normalizedOcrScore, fieldsScore);

        return Math.Round(documentScore, 2);
    }

    private static decimal NormalizeConfidenceScore(decimal rawConfidence)
    {
        // Normalize 0-1 range to 0-100 range if necessary
        decimal normalizedScore = rawConfidence <= 1.0m ? rawConfidence * MaxScore : rawConfidence;
        return Math.Clamp(normalizedScore, MinScore, MaxScore);
    }

    private static decimal CalculateRequiredFieldsScore(IEnumerable<ExtractedResultDto> extractedFields, IEnumerable<TemplateFieldDto> templateFields)
    {
        var requiredFields = templateFields.Where(f => f.Required).ToList();
        
        if (!requiredFields.Any())
        {
            return MaxScore; // If no required fields exist, we assume 100% success for this metric
        }

        decimal totalFieldConfidence = 0;
        
        foreach (var reqField in requiredFields)
        {
            var extracted = extractedFields.FirstOrDefault(e => e.FieldName == reqField.FieldName);
            
            if (IsValidExtraction(extracted))
            {
                totalFieldConfidence += GetNormalizedFieldConfidence(extracted!.Confidence);
            }
        }
        
        return totalFieldConfidence / requiredFields.Count;
    }

    private static bool IsValidExtraction(ExtractedResultDto? extracted)
    {
        return extracted != null && !string.IsNullOrWhiteSpace(extracted.RawValue);
    }

    private static decimal GetNormalizedFieldConfidence(decimal confidence)
    {
        decimal normalizedConf = NormalizeConfidenceScore(confidence);
        
        // If strategy found the data but didn't assign a confidence score, we assume 100%
        return normalizedConf == 0 ? DefaultFieldConfidence : normalizedConf;
    }

    private static decimal CalculateWeightedFinalScore(decimal ocrScore, decimal fieldsScore)
    {
        return (ocrScore * OcrScoreWeight) + (fieldsScore * FieldsScoreWeight);
    }
}
