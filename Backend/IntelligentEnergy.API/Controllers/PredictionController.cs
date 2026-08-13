using IntelligentEnergy.API.DTOs;
using IntelligentEnergy.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentEnergy.API.Controllers;

[ApiController]
[Route("api/prediction")]
public class PredictionController : ControllerBase
{
    private readonly PredictionService _service;

    public PredictionController(PredictionService service)
    {
        _service = service;
    }

    /// <summary>Calls the Python AI service to predict energy consumption.</summary>
    [HttpPost]
    public async Task<IActionResult> Predict([FromBody] PredictionRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.PredictAsync(request);
        return Ok(result);
    }

    /// <summary>Returns recent prediction history.</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 20)
    {
        return Ok(await _service.GetHistoryAsync(limit));
    }
}
