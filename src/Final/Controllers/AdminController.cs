using Final.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IPropiedadService _propiedadService;
    private readonly IUsuarioService _usuarioService;
    private readonly IDashboardService _dashboardService;

    public AdminController(
        IPropiedadService propiedadService,
        IUsuarioService usuarioService,
        IDashboardService dashboardService)
    {
        _propiedadService = propiedadService;
        _usuarioService = usuarioService;
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Obtener propiedades pendientes de aprobación
    /// </summary>
    [HttpGet("propiedades-pendientes")]
    public async Task<IActionResult> GetPropiedadesPendientes()
    {
        try
        {
            var propiedades = await _propiedadService.GetPendientesAprobacionAsync();
            return Ok(propiedades);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Aprobar una propiedad
    /// </summary>
    [HttpPut("propiedades/{id}/approve")]
    public async Task<IActionResult> ApproveProperty(int id)
    {
        try
        {
            var ok = await _propiedadService.AprobarPropiedadAsync(id, true);
            if (!ok)
                return BadRequest(new { error = "No se pudo aprobar la propiedad" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Rechazar una propiedad
    /// </summary>
    [HttpPut("propiedades/{id}/reject")]
    public async Task<IActionResult> RejectProperty(int id)
    {
        try
        {
            var ok = await _propiedadService.AprobarPropiedadAsync(id, false);
            if (!ok)
                return BadRequest(new { error = "No se pudo rechazar la propiedad" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener todos los usuarios
    /// </summary>
    [HttpGet("usuarios")]
    public async Task<IActionResult> GetAllUsuarios()
    {
        try
        {
            var usuarios = await _usuarioService.GetAllAsync();
            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Desactivar/activar un usuario
    /// </summary>
    [HttpPut("usuarios/{id}/toggle")]
    public async Task<IActionResult> ToggleUsuario(int id, [FromBody] bool activo)
    {
        try
        {
            var ok = await _usuarioService.ToggleActivoAsync(id, activo);
            if (!ok)
                return BadRequest(new { error = "No se pudo actualizar el usuario" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener estadísticas del sistema (dashboard)
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var stats = await _dashboardService.GetStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
