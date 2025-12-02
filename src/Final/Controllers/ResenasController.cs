using Final.DTOs.Resena;
using Final.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ResenasController : ControllerBase
{
    private readonly IResenaService _resenaService;

    public ResenasController(IResenaService resenaService)
    {
        _resenaService = resenaService;
    }

    /// <summary>
    /// Crear una nueva reseña (solo después de completar una reserva)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ResenaCreateDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var resena = await _resenaService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = resena.Id }, resena);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener detalle de una reseña por ID
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var resena = await _resenaService.GetByIdAsync(id);
            return Ok(resena);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener todas las reseñas de una propiedad
    /// </summary>
    [AllowAnonymous]
    [HttpGet("propiedad/{propiedadId}")]
    public async Task<IActionResult> GetByPropiedad(int propiedadId)
    {
        try
        {
            var resenas = await _resenaService.GetByPropiedadAsync(propiedadId);
            return Ok(resenas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener mis reseñas (como usuario autenticado)
    /// </summary>
    [HttpGet("my-resenas")]
    public async Task<IActionResult> GetMyResenas()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var resenas = await _resenaService.GetByUsuarioAsync(userId);
            return Ok(resenas);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener calificación promedio de una propiedad
    /// </summary>
    [AllowAnonymous]
    [HttpGet("propiedad/{propiedadId}/promedio")]
    public async Task<IActionResult> GetPromedioCalificacion(int propiedadId)
    {
        try
        {
            var promedio = await _resenaService.GetCalificacionPromedioAsync(propiedadId);
            return Ok(new { propiedadId, promedio });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar una reseña (solo el autor)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ResenaCreateDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var resena = await _resenaService.UpdateAsync(id, dto, userId);
            return Ok(resena);
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
    /// Eliminar una reseña (solo el autor)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var ok = await _resenaService.DeleteAsync(id, userId);
            if (!ok)
                return BadRequest(new { error = "No se pudo eliminar la reseña" });

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
