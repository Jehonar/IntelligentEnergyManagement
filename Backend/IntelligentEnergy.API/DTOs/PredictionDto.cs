namespace IntelligentEnergy.API.DTOs;

public class PredictionRequestDto
{
    public int Hour { get; set; }
    public int DayOfWeek { get; set; }
    public int Month { get; set; }
    public double Temperature { get; set; }
    public double PreviousConsumption { get; set; }
}

public class PredictionResponseDto
{
    public int Id { get; set; }
    public string PredictionDate { get; set; } = string.Empty;
    public int PredictionHour { get; set; }
    public decimal PredictedConsumption { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public class PredictionHistoryDto
{
    public int Id { get; set; }
    public string PredictionDate { get; set; } = string.Empty;
    public int PredictionHour { get; set; }
    public decimal PredictedConsumption { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
