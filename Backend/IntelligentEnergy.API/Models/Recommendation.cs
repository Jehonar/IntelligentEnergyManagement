namespace IntelligentEnergy.API.Models;

public class Recommendation
{
    public int Id { get; set; }
    public int? PredictionId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string RecommendationType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Prediction? Prediction { get; set; }
}
