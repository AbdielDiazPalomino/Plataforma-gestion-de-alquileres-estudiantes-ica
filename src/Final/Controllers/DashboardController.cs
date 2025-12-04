// Controllers/DashboardController.cs
using Final.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Final.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Solo administradores
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// GET: Obtener estadísticas generales del sistema
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var stats = await _dashboardService.GetStatsAsync();
            return Ok(stats);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET: Obtener las propiedades más populares
    /// </summary>
    [HttpGet("top-propiedades")]
    public async Task<IActionResult> GetTopPropiedades()
    {
        try
        {
            var topPropiedades = await _dashboardService.GetTopPropiedadesAsync();
            return Ok(topPropiedades);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }
}