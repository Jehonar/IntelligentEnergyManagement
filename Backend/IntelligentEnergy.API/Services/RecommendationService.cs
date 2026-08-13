using IntelligentEnergy.API.Data;
using IntelligentEnergy.API.DTOs;
using IntelligentEnergy.API.Models;
using Microsoft.EntityFrameworkCore;

namespace IntelligentEnergy.API.Services;

public class RecommendationService
{
    private readonly ApplicationDbContext _db;

    public RecommendationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<RecommendationDto> GenerateAsync(RecommendationRequestDto request)
    {
        var averageConsumption = await _db.EnergyReadings
            .AverageAsync(r => (double)r.EnergyConsumption);

        var type    = DetermineType((double)request.PredictedConsumption, averageConsumption);
        var message = BuildMessage(type, (double)request.PredictedConsumption, averageConsumption);

        var entity = new Recommendation
        {
            PredictionId       = request.PredictionId,
            Message            = message,
            RecommendationType = type
        };

        _db.Recommendations.Add(entity);
        await _db.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<List<RecommendationDto>> GetRecentAsync(int limit = 10)
    {
        return await _db.Recommendations
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .Select(r => new RecommendationDto
            {
                Id                 = r.Id,
                PredictionId       = r.PredictionId,
                Message            = r.Message,
                RecommendationType = r.RecommendationType,
                CreatedAt          = r.CreatedAt.ToString("o")
            })
            .ToListAsync();
    }

    private static string DetermineType(double predicted, double average)
    {
        if (predicted > average * 1.20) return "HIGH";
        if (predicted > average * 1.05) return "MODERATE";
        if (predicted <= average * 0.90) return "LOW";
        return "NORMAL";
    }

    private static string BuildMessage(string type, double predicted, double average)
    {
        var pct = average > 0 ? (int)Math.Round((predicted - average) / average * 100) : 0;

        return type switch
        {
            "HIGH" =>
                $"High energy consumption detected. The predicted value of {predicted:F2} kWh " +
                $"is approximately {pct}% above the historical average ({average:F2} kWh). " +
                "Consider reducing the use of high-consumption devices during peak hours, " +
                "such as HVAC systems, and shifting non-essential loads to off-peak times.",
            "MODERATE" =>
                $"Energy consumption is slightly elevated. The predicted value of {predicted:F2} kWh " +
                $"is about {pct}% above the historical average ({average:F2} kWh). " +
                "Monitor your usage and consider turning off unused devices.",
            "NORMAL" =>
                $"Energy consumption is within the expected range ({predicted:F2} kWh). " +
                "Continue monitoring your current usage to maintain efficiency.",
            "LOW" =>
                $"Great job! Your predicted energy consumption ({predicted:F2} kWh) is lower than " +
                $"the historical average ({average:F2} kWh). Keep up the efficient usage habits.",
            _ => "Unable to generate a recommendation at this time."
        };
    }

    private static RecommendationDto MapToDto(Recommendation r) => new()
    {
        Id                 = r.Id,
        PredictionId       = r.PredictionId,
        Message            = r.Message,
        RecommendationType = r.RecommendationType,
        CreatedAt          = r.CreatedAt.ToString("o")
    };
}
