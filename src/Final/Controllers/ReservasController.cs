using Final.DTOs.Reserva;
using Final.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    /// <summary>
    /// Crear una nueva reserva (requiere autenticación)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservaCreateDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var reserva = await _reservaService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = reserva.Id }, reserva);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener detalle de una reserva por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var reserva = await _reservaService.GetByIdAsync(id);
            return Ok(reserva);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener mis reservas como usuario (búsquedas que hice)
    /// </summary>
    [HttpGet("my-reservas")]
    public async Task<IActionResult> GetMyReservas()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var reservas = await _reservaService.GetByUsuarioAsync(userId);
            return Ok(reservas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener reservas de mis propiedades (como propietario)
    /// </summary>
    [HttpGet("property-reservas")]
    public async Task<IActionResult> GetPropertyReservas()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var propietarioId))
                return Unauthorized();

            var reservas = await _reservaService.GetByPropietarioAsync(propietarioId);
            return Ok(reservas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Verificar disponibilidad de una propiedad en rango de fechas
    /// </summary>
    [HttpGet("disponibilidad")]
    public async Task<IActionResult> VerificarDisponibilidad(
        [FromQuery] int propiedadId,
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin)
    {
        try
        {
            var disponible = await _reservaService.VerificarDisponibilidadAsync(propiedadId, fechaInicio, fechaFin);
            return Ok(new { disponible, propiedadId, fechaInicio, fechaFin });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancelar una reserva
    /// </summary>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var ok = await _reservaService.CancelAsync(id, userId);
            if (!ok)
                return BadRequest(new { error = "No se pudo cancelar la reserva" });

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Confirmar una reserva (solo propietario)
    /// </summary>
    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var propietarioId))
                return Unauthorized();

            var ok = await _reservaService.ConfirmarReservaAsync(id, propietarioId);
            if (!ok)
                return BadRequest(new { error = "No se pudo confirmar la reserva" });

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
