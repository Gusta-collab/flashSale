using FlashSale.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlashSale.Api.Controllers;

/// <summary>
/// Controller para health checks.
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Health check básico.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Health check com verificação de dependências.
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready()
    {
        var checks = new Dictionary<string, string>();

        // Check PostgreSQL
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            checks["postgresql"] = "Healthy";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostgreSQL health check failed");
            checks["postgresql"] = "Unhealthy";
        }

        // TODO: Check Redis quando implementado

        var isHealthy = checks.Values.All(v => v == "Healthy");

        if (!isHealthy)
        {
            return StatusCode(503, new { status = "Unhealthy", checks });
        }

        return Ok(new { status = "Healthy", checks, timestamp = DateTime.UtcNow });
    }
}
