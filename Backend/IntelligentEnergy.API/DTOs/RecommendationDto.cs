namespace IntelligentEnergy.API.DTOs;

public class RecommendationDto
{
    public int Id { get; set; }
    public int? PredictionId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string RecommendationType { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

public class RecommendationRequestDto
{
    public decimal PredictedConsumption { get; set; }
    public int? PredictionId { get; set; }
}
