using Final.DTOs.Resena;
using Final.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResenasController : ControllerBase
{
    private readonly IResenaService _resenaService;

    public ResenasController(IResenaService resenaService)
    {
        _resenaService = resenaService;
    }

    /// <summary>
    /// GET: Obtener todas las reseñas de una propiedad (público)
    /// </summary>
    [HttpGet("propiedad/{propiedadId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByPropiedad(int propiedadId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? minRating = null,
        [FromQuery] int? maxRating = null)
    {
        try
        {
            var resenas = await _resenaService.GetByPropiedadAsync(propiedadId);

            // Aplicar filtros de calificación
            if (minRating.HasValue)
                resenas = resenas.Where(r => r.Calificacion >= minRating.Value).ToList();

            if (maxRating.HasValue)
                resenas = resenas.Where(r => r.Calificacion <= maxRating.Value).ToList();

            var total = resenas.Count;
            var paginated = resenas
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var promedio = await _resenaService.GetCalificacionPromedioAsync(propiedadId);

            return Ok(new
            {
                resenas = paginated,
                estadisticas = new
                {
                    promedio,
                    totalResenas = total,
                    distribucion = resenas.GroupBy(r => r.Calificacion)
                        .Select(g => new { calificacion = g.Key, cantidad = g.Count() })
                        .OrderBy(x => x.calificacion)
                },
                pagination = new
                {
                    page,
                    pageSize,
                    total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                }
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET: Obtener reseña por ID (público)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var resena = await _resenaService.GetByIdAsync(id);
            return Ok(resena);
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
    /// GET: Obtener mis reseñas (autenticado)
    /// </summary>
    [HttpGet("my-resenas")]
    [Authorize]
    public async Task<IActionResult> GetMyResenas()
    {
        try
        {
            var userId = GetCurrentUserId();
            var resenas = await _resenaService.GetByUsuarioAsync(userId);
            return Ok(resenas);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET: Obtener calificación promedio de una propiedad (público)
    /// </summary>
    [HttpGet("propiedad/{propiedadId}/promedio")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPromedioCalificacion(int propiedadId)
    {
        try
        {
            var promedio = await _resenaService.GetCalificacionPromedioAsync(propiedadId);
            return Ok(new
            {
                propiedadId,
                promedio,
                calificacionTexto = promedio switch
                {
                    >= 4.5 => "Excelente",
                    >= 4.0 => "Muy bueno",
                    >= 3.0 => "Bueno",
                    >= 2.0 => "Regular",
                    _ => "Malo"
                }
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// POST: Crear nueva reseña (autenticado, solo después de reserva completada)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] ResenaCreateDto dto)
    {
        try
        {
            // Validar calificación
            if (dto.Calificacion < 1 || dto.Calificacion > 5)
                return BadRequest(new { error = "La calificación debe estar entre 1 y 5" });

            // Validar comentario
            if (string.IsNullOrWhiteSpace(dto.Comentario) || dto.Comentario.Length < 10)
                return BadRequest(new { error = "El comentario debe tener al menos 10 caracteres" });

            var userId = GetCurrentUserId();
            var resena = await _resenaService.CreateAsync(dto, userId);

            // Recalcular promedio después de crear
            var nuevoPromedio = await _resenaService.GetCalificacionPromedioAsync(dto.PropiedadId);

            return CreatedAtAction(nameof(GetById), new { id = resena.Id }, new
            {
                resena,
                estadisticas = new { nuevoPromedio }
            });
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
    /// PUT: Actualizar reseña (solo autor)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] ResenaUpdateDto dto)
    {
        try
        {
            // Validar calificación
            if (dto.Calificacion < 1 || dto.Calificacion > 5)
                return BadRequest(new { error = "La calificación debe estar entre 1 y 5" });

            var userId = GetCurrentUserId();
            var resena = await _resenaService.UpdateAsync(id, dto, userId);

            // Recalcular promedio después de actualizar
            var nuevoPromedio = await _resenaService.GetCalificacionPromedioAsync(resena.PropiedadId);

            return Ok(new
            {
                resena,
                estadisticas = new { nuevoPromedio }
            });
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
    /// DELETE: Eliminar reseña (solo autor o admin)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            // Usar el servicio para eliminar
            var success = await _resenaService.DeleteAsync(id, userId);
            if (!success)
                return BadRequest(new { error = "No se pudo eliminar la reseña" });

            // Obtener propiedadId para recalcular promedio
            var resena = await _resenaService.GetByIdAsync(id);
            var nuevoPromedio = await _resenaService.GetCalificacionPromedioAsync(resena.PropiedadId);

            return Ok(new
            {
                message = "Reseña eliminada exitosamente",
                estadisticas = new { nuevoPromedio }
            });
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
    /// GET: Obtener estadísticas detalladas de reseñas de una propiedad (público)
    /// </summary>
    [HttpGet("propiedad/{propiedadId}/estadisticas")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEstadisticas(int propiedadId)
    {
        try
        {
            var resenas = await _resenaService.GetByPropiedadAsync(propiedadId);

            if (!resenas.Any())
                return Ok(new
                {
                    propiedadId,
                    message = "No hay reseñas para esta propiedad",
                    estadisticas = new
                    {
                        promedio = 0,
                        total = 0,
                        distribucion = new int[5]
                    }
                });

            var promedio = await _resenaService.GetCalificacionPromedioAsync(propiedadId);
            var distribucion = Enumerable.Range(1, 5)
                .Select(rating => new
                {
                    calificacion = rating,
                    cantidad = resenas.Count(r => r.Calificacion == rating),
                    porcentaje = (resenas.Count(r => r.Calificacion == rating) / (double)resenas.Count) * 100
                })
                .ToList();

            return Ok(new
            {
                propiedadId,
                estadisticas = new
                {
                    promedio,
                    totalResenas = resenas.Count,
                    mejorCalificacion = resenas.Max(r => r.Calificacion),
                    peorCalificacion = resenas.Min(r => r.Calificacion),
                    distribucion
                }
            });
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