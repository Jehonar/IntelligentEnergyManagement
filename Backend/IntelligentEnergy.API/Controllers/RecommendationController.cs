using IntelligentEnergy.API.DTOs;
using IntelligentEnergy.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentEnergy.API.Controllers;

[ApiController]
[Route("api/recommendations")]
public class RecommendationController : ControllerBase
{
    private readonly RecommendationService _service;

    public RecommendationController(RecommendationService service)
    {
        _service = service;
    }

    /// <summary>Generates an AI recommendation based on predicted consumption.</summary>
    [HttpPost]
    public async Task<IActionResult> Generate([FromBody] RecommendationRequestDto request)
    {
        var result = await _service.GenerateAsync(request);
        return Ok(result);
    }

    /// <summary>Returns recent recommendations.</summary>
    [HttpGet]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 10)
    {
        return Ok(await _service.GetRecentAsync(limit));
    }
}
