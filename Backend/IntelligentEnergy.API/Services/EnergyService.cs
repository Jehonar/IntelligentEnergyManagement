using IntelligentEnergy.API.Data;
using IntelligentEnergy.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IntelligentEnergy.API.Services;

public class EnergyService
{
    private readonly ApplicationDbContext _db;

    public EnergyService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<EnergyReadingDto>> GetReadingsAsync(
        DateOnly? from, DateOnly? to, string? device, int pageSize = 200)
    {
        var query = _db.EnergyReadings.AsQueryable();

        if (from.HasValue) query = query.Where(r => r.ReadingDate >= from.Value);
        if (to.HasValue)   query = query.Where(r => r.ReadingDate <= to.Value);
        if (!string.IsNullOrEmpty(device)) query = query.Where(r => r.DeviceName == device);

        return await query
            .OrderByDescending(r => r.ReadingDate)
            .ThenByDescending(r => r.ReadingHour)
            .Take(pageSize)
            .Select(r => new EnergyReadingDto
            {
                Id = r.Id,
                ReadingDate = r.ReadingDate.ToString("yyyy-MM-dd"),
                ReadingHour = r.ReadingHour,
                EnergyConsumption = r.EnergyConsumption,
                Temperature = r.Temperature,
                DeviceName = r.DeviceName
            })
            .ToListAsync();
    }

    public async Task<List<DailyConsumptionDto>> GetDailyAsync(int days = 30)
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));

        return await _db.EnergyReadings
            .Where(r => r.ReadingDate >= from)
            .GroupBy(r => r.ReadingDate)
            .Select(g => new DailyConsumptionDto
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                TotalConsumption = g.Sum(r => r.EnergyConsumption)
            })
            .OrderBy(d => d.Date)
            .ToListAsync();
    }

    public async Task<List<MonthlyConsumptionDto>> GetMonthlyAsync()
    {
        var months = new[]
        {
            "January","February","March","April","May","June",
            "July","August","September","October","November","December"
        };

        return await _db.EnergyReadings
            .GroupBy(r => new { r.ReadingDate.Year, r.ReadingDate.Month })
            .Select(g => new MonthlyConsumptionDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalConsumption = g.Sum(r => r.EnergyConsumption)
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToListAsync();
    }

    public async Task<EnergyStatisticsDto> GetStatisticsAsync()
    {
        var all = await _db.EnergyReadings.ToListAsync();
        if (!all.Any())
            return new EnergyStatisticsDto();

        var dailyGroups = all.GroupBy(r => r.ReadingDate).ToList();

        var stats = new EnergyStatisticsDto
        {
            TotalConsumption       = all.Sum(r => r.EnergyConsumption),
            AverageDailyConsumption = dailyGroups.Average(g => g.Sum(r => r.EnergyConsumption)),
            LatestConsumption      = all.OrderByDescending(r => r.ReadingDate).ThenByDescending(r => r.ReadingHour).First().EnergyConsumption,
            HighestConsumption     = all.Max(r => r.EnergyConsumption),
            LowestConsumption      = all.Min(r => r.EnergyConsumption),
            DailyData = dailyGroups
                .OrderByDescending(g => g.Key)
                .Take(30)
                .Select(g => new DailyConsumptionDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    TotalConsumption = g.Sum(r => r.EnergyConsumption)
                })
                .OrderBy(d => d.Date)
                .ToList(),
            HourlyData = all
                .GroupBy(r => r.ReadingHour)
                .Select(g => new HourlyConsumptionDto
                {
                    Hour = g.Key,
                    AverageConsumption = g.Average(r => r.EnergyConsumption)
                })
                .OrderBy(h => h.Hour)
                .ToList()
        };

        // Monthly data
        var months = new[]
        {
            "January","February","March","April","May","June",
            "July","August","September","October","November","December"
        };
        stats.MonthlyData = all
            .GroupBy(r => new { r.ReadingDate.Year, r.ReadingDate.Month })
            .Select(g => new MonthlyConsumptionDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = months[g.Key.Month - 1],
                TotalConsumption = g.Sum(r => r.EnergyConsumption)
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        return stats;
    }

    public async Task<List<string>> GetDevicesAsync()
    {
        return await _db.EnergyReadings
            .Select(r => r.DeviceName)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }
}
