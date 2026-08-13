namespace IntelligentEnergy.API.DTOs;

public class EnergyReadingDto
{
    public int Id { get; set; }
    public string ReadingDate { get; set; } = string.Empty;
    public int ReadingHour { get; set; }
    public decimal EnergyConsumption { get; set; }
    public decimal? Temperature { get; set; }
    public string DeviceName { get; set; } = string.Empty;
}

public class EnergyStatisticsDto
{
    public decimal TotalConsumption { get; set; }
    public decimal AverageDailyConsumption { get; set; }
    public decimal LatestConsumption { get; set; }
    public decimal HighestConsumption { get; set; }
    public decimal LowestConsumption { get; set; }
    public List<DailyConsumptionDto> DailyData { get; set; } = new();
    public List<HourlyConsumptionDto> HourlyData { get; set; } = new();
    public List<MonthlyConsumptionDto> MonthlyData { get; set; } = new();
}

public class DailyConsumptionDto
{
    public string Date { get; set; } = string.Empty;
    public decimal TotalConsumption { get; set; }
}

public class HourlyConsumptionDto
{
    public int Hour { get; set; }
    public decimal AverageConsumption { get; set; }
}

public class MonthlyConsumptionDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalConsumption { get; set; }
}
