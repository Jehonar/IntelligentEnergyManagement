namespace IntelligentEnergy.API.Models;

public class EnergyReading
{
    public int Id { get; set; }
    public DateOnly ReadingDate { get; set; }
    public int ReadingHour { get; set; }
    public decimal EnergyConsumption { get; set; }
    public decimal? Temperature { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
