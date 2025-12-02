using Final.DTOs.Propiedad;
using Final.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropiedadesController : ControllerBase
{
    private readonly IPropiedadService _propiedadService;

    public PropiedadesController(IPropiedadService propiedadService)
    {
        _propiedadService = propiedadService;
    }

    /// <summary>
    /// Obtener todas las propiedades aprobadas (sin autenticación)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var propiedades = await _propiedadService.GetAllAsync();
            return Ok(propiedades);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener detalle de una propiedad por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var propiedad = await _propiedadService.GetByIdAsync(id);
            return Ok(propiedad);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Buscar propiedades con filtros (distrito, precio, habitaciones)
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] decimal? precioMin,
        [FromQuery] decimal? precioMax,
        [FromQuery] int? habitaciones,
        [FromQuery] string distrito = null,
        [FromQuery] bool? amoblado = null)
    {
        try
        {
            var propiedades = await _propiedadService.GetByFiltersAsync(precioMin, precioMax, habitaciones, distrito, amoblado);
            return Ok(propiedades);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Crear una nueva propiedad (requiere autenticación)
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PropiedadCreateDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var propiedad = await _propiedadService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = propiedad.Id }, propiedad);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar una propiedad (solo el propietario)
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PropiedadUpdateDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var propiedad = await _propiedadService.UpdateAsync(id, dto, userId);
            return Ok(propiedad);
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
    /// Eliminar una propiedad (solo el propietario)
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var ok = await _propiedadService.DeleteAsync(id, userId);
            if (!ok)
                return BadRequest(new { error = "No se pudo eliminar la propiedad" });

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
    /// Obtener mis propiedades (requiere autenticación)
    /// </summary>
    [Authorize]
    [HttpGet("my-properties")]
    public async Task<IActionResult> GetMyProperties()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var propiedades = await _propiedadService.GetByPropietarioAsync(userId);
            return Ok(propiedades);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
