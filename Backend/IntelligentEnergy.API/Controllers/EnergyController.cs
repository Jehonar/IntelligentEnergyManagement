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
        DateOnly? fromDate = from is not null ? DateOnly.Parse(from) : null;
        DateOnly? toDate   = to   is not null ? DateOnly.Parse(to)   : null;

        var data = await _service.GetReadingsAsync(fromDate, toDate, device);
        return Ok(data);
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
