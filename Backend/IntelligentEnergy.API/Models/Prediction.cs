namespace IntelligentEnergy.API.Models;

public class Prediction
{
    public int Id { get; set; }
    public DateOnly PredictionDate { get; set; }
    public int PredictionHour { get; set; }
    public decimal PredictedConsumption { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
