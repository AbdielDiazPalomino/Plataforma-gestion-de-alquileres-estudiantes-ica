using Final.DTOs.Reserva;
using Final.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    /// <summary>
    /// GET: Obtener todas mis reservas como inquilino
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyReservas()
    {
        try
        {
            var userId = GetCurrentUserId();
            var reservas = await _reservaService.GetByUsuarioAsync(userId);
            return Ok(reservas);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET: Obtener reserva por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var reserva = await _reservaService.GetByIdAsync(id);
            
            // Verificar permisos
            if (reserva == null)
                return NotFound(new { error = "Reserva no encontrada" });

            return Ok(reserva);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// POST: Crear nueva reserva
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservaCreateDto dto)
    {
        try
        {
            // Validar fechas
            if (dto.FechaFin <= dto.FechaInicio)
                return BadRequest(new { error = "La fecha de fin debe ser posterior a la fecha de inicio" });

             // CORRECCIÓN: dto.FechaInicio ya es DateTime, solo compara fechas (sin hora)
            if (dto.FechaInicio.Date < DateTime.Today)
                return BadRequest(new { error = "No se pueden hacer reservas en el pasado" });

            var userId = GetCurrentUserId();
            var reserva = await _reservaService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = reserva.Id }, reserva);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// PUT: Actualizar reserva (solo inquilino antes de confirmación)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReservaCreateDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            // Necesitarías un método UpdateAsync en el servicio
            // var reserva = await _reservaService.UpdateAsync(id, dto, userId);
            // return Ok(reserva);
            return StatusCode(501, new { error = "Funcionalidad en desarrollo" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// DELETE: Cancelar reserva
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _reservaService.CancelAsync(id, userId);
            
            if (!success)
                return BadRequest(new { error = "No se pudo cancelar la reserva" });

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET: Verificar disponibilidad de una propiedad
    /// </summary>
    [HttpGet("check-availability")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckAvailability(
        [FromQuery] int propiedadId,
        [FromQuery] DateOnly fechaInicio,
        [FromQuery] DateOnly fechaFin)
    {
        try
        {
            if (fechaFin <= fechaInicio)
                return BadRequest(new { error = "La fecha de fin debe ser posterior a la fecha de inicio" });

            // Convertir DateOnly a DateTime
            var fechaInicioDateTime = fechaInicio.ToDateTime(TimeOnly.MinValue);
            var fechaFinDateTime = fechaFin.ToDateTime(TimeOnly.MinValue);

            var disponible = await _reservaService.VerificarDisponibilidadAsync(
                propiedadId, 
                fechaInicioDateTime, 
                fechaFinDateTime);
            
            return Ok(new 
            { 
                disponible, 
                propiedadId, 
                fechaInicio, 
                fechaFin,
                message = disponible ? "Propiedad disponible" : "Propiedad no disponible en esas fechas"
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET: Calcular precio de reserva sin crearla
    /// </summary>
    [HttpGet("calculate-price")]
    [AllowAnonymous]
    public async Task<IActionResult> CalculatePrice(
        [FromQuery] int propiedadId,
        [FromQuery] DateOnly fechaInicio,
        [FromQuery] DateOnly fechaFin)
    {
        try
        {
            if (fechaFin <= fechaInicio)
                return BadRequest(new { error = "La fecha de fin debe ser posterior a la fecha de inicio" });

            // Convertir DateOnly a DateTime
            var fechaInicioDateTime = fechaInicio.ToDateTime(TimeOnly.MinValue);
            var fechaFinDateTime = fechaFin.ToDateTime(TimeOnly.MinValue);

            // Verificar disponibilidad primero
            var disponible = await _reservaService.VerificarDisponibilidadAsync(
                propiedadId, 
                fechaInicioDateTime, 
                fechaFinDateTime);
            
            if (!disponible)
                return BadRequest(new { error = "Propiedad no disponible en esas fechas" });

            // Calcular precio
            var dias = (fechaFinDateTime - fechaInicioDateTime).Days;
            // Aquí deberías obtener el precio mensual de la propiedad
            // var propiedad = await _propiedadService.GetByIdAsync(propiedadId);
            // var precioTotal = (propiedad.PrecioMensual / 30) * dias;
            
            return Ok(new 
            { 
                propiedadId, 
                fechaInicio, 
                fechaFin,
                dias,
                precioTotal = 0 // Placeholder - necesitas implementar este cálculo
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET: Obtener reservas de mis propiedades (como propietario)
    /// </summary>
    [HttpGet("my-properties-reservas")]
    public async Task<IActionResult> GetMyPropertiesReservas()
    {
        try
        {
            var userId = GetCurrentUserId();
            var reservas = await _reservaService.GetByPropietarioAsync(userId);
            return Ok(reservas);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// PUT: Confirmar reserva (solo propietario)
    /// </summary>
    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _reservaService.ConfirmarReservaAsync(id, userId);
            
            if (!success)
                return BadRequest(new { error = "No se pudo confirmar la reserva" });

            return Ok(new { message = "Reserva confirmada exitosamente" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// PUT: Rechazar reserva (solo propietario)
    /// </summary>
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            // Necesitarías un método RejectAsync en el servicio
            // var success = await _reservaService.RejectAsync(id, userId);
            // return Ok(new { message = "Reserva rechazada" });
            return StatusCode(501, new { error = "Funcionalidad en desarrollo" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException();
        return userId;
    }
}