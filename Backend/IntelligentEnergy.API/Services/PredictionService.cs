using System.Net.Http.Json;
using IntelligentEnergy.API.Data;
using IntelligentEnergy.API.DTOs;
using IntelligentEnergy.API.Models;
using Microsoft.EntityFrameworkCore;

namespace IntelligentEnergy.API.Services;

public class PredictionService
{
    private readonly ApplicationDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PredictionService> _logger;

    public PredictionService(ApplicationDbContext db, IHttpClientFactory factory, ILogger<PredictionService> logger)
    {
        _db = db;
        _httpClient = factory.CreateClient("AiService");
        _logger = logger;
    }

    public async Task<PredictionResponseDto> PredictAsync(PredictionRequestDto request)
    {
        double predicted;

        try
        {
            var aiResponse = await _httpClient.PostAsJsonAsync("/predict", request);
            aiResponse.EnsureSuccessStatusCode();

            var result = await aiResponse.Content.ReadFromJsonAsync<AiPredictResponse>();
            predicted = result?.PredictedConsumption ?? FallbackPredict(request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI service unavailable, using fallback prediction.");
            predicted = FallbackPredict(request);
        }

        var entity = new Prediction
        {
            PredictionDate       = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            PredictionHour       = request.Hour,
            PredictedConsumption = (decimal)Math.Round(predicted, 4)
        };

        _db.Predictions.Add(entity);
        await _db.SaveChangesAsync();

        return new PredictionResponseDto
        {
            Id                   = entity.Id,
            PredictionDate       = entity.PredictionDate.ToString("yyyy-MM-dd"),
            PredictionHour       = entity.PredictionHour,
            PredictedConsumption = entity.PredictedConsumption,
            CreatedAt            = entity.CreatedAt.ToString("o")
        };
    }

    public async Task<List<PredictionHistoryDto>> GetHistoryAsync(int limit = 20)
    {
        return await _db.Predictions
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .Select(p => new PredictionHistoryDto
            {
                Id                   = p.Id,
                PredictionDate       = p.PredictionDate.ToString("yyyy-MM-dd"),
                PredictionHour       = p.PredictionHour,
                PredictedConsumption = p.PredictedConsumption,
                CreatedAt            = p.CreatedAt.ToString("o")
            })
            .ToListAsync();
    }

    // Rule-based fallback when the Python AI service is not available
    private static double FallbackPredict(PredictionRequestDto r)
    {
        double base_ = r.PreviousConsumption > 0 ? r.PreviousConsumption : 10.0;

        double hourFactor = r.Hour switch
        {
            >= 0 and <= 5   => 0.6,
            >= 6 and <= 8   => 1.4,
            >= 9 and <= 17  => 1.0,
            >= 18 and <= 21 => 1.6,
            _               => 0.9
        };

        double tempFactor = r.Temperature > 25 ? 1.15 : r.Temperature < 5 ? 1.20 : 1.0;
        double weekendFactor = r.DayOfWeek is 0 or 6 ? 0.75 : 1.0;

        return base_ * hourFactor * tempFactor * weekendFactor;
    }

    private class AiPredictResponse
    {
        public double PredictedConsumption { get; set; }
    }
}
