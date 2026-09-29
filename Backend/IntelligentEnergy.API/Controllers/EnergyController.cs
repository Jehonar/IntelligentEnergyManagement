using System.Globalization;
using IntelligentEnergy.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentEnergy.API.Controllers;

[ApiController]
[Route("api/energy")]
public class EnergyController : ControllerBase
{
    private readonly EnergyService _service;

    public EnergyController(EnergyService service)
    {
        _service = service;
    }

    /// <summary>Returns paginated energy readings with optional filters.</summary>
    [HttpGet]
    public async Task<IActionResult> GetReadings(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? device)
    {
        if (!TryReadDate(from, out var fromDate))
            return BadRequest(new { message = "Invalid from date. Use yyyy-MM-dd." });
        if (!TryReadDate(to, out var toDate))
            return BadRequest(new { message = "Invalid to date. Use yyyy-MM-dd." });

        var data = await _service.GetReadingsAsync(fromDate, toDate, device);
        return Ok(data);
    }

    private static bool TryReadDate(string? value, out DateOnly? date)
    {
        date = null;
        if (string.IsNullOrWhiteSpace(value))
            return true;

        if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return false;

        date = parsed;
        return true;
    }

    /// <summary>Returns daily total consumption for the last N days.</summary>
    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily([FromQuery] int days = 30)
    {
        return Ok(await _service.GetDailyAsync(days));
    }

    /// <summary>Returns monthly total consumption.</summary>
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly()
    {
        return Ok(await _service.GetMonthlyAsync());
    }

    /// <summary>Returns aggregated statistics and chart data.</summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        return Ok(await _service.GetStatisticsAsync());
    }

    /// <summary>Returns the list of distinct device names.</summary>
    [HttpGet("devices")]
    public async Task<IActionResult> GetDevices()
    {
        return Ok(await _service.GetDevicesAsync());
    }
}
