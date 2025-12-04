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
    private readonly IWebHostEnvironment _environment;

    public PropiedadesController(IPropiedadService propiedadService, IWebHostEnvironment environment)
    {
        _propiedadService = propiedadService;
        _environment = environment;
    }

    /// <summary>
    /// GET: Obtener todas las propiedades aprobadas (público)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var propiedades = await _propiedadService.GetAllAsync();
            var total = propiedades.Count;
            var paginated = propiedades
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                propiedades = paginated,
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
    /// GET: Obtener propiedad por ID (público)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var propiedad = await _propiedadService.GetByIdAsync(id);
            return Ok(propiedad);
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
    /// GET: Buscar propiedades con filtros (público)
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? distrito,
        [FromQuery] decimal? precioMin,
        [FromQuery] decimal? precioMax,
        [FromQuery] int? habitaciones,
        [FromQuery] bool? amoblado,
        [FromQuery] int? banos,
        [FromQuery] bool? aceptaMascotas,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var propiedades = await _propiedadService.GetByFiltersAsync(
                precioMin, precioMax, habitaciones, distrito ?? string.Empty, amoblado);
            
            // Primero convertimos a lista para evitar problemas con la evaluación diferida
            var propiedadList = propiedades.ToList();
            
            // Aplicar filtros adicionales
            if (banos.HasValue)
                propiedadList = propiedadList.Where(p => p.Banos >= banos.Value).ToList();
            
            if (aceptaMascotas.HasValue)
                propiedadList = propiedadList.Where(p => p.AceptaMascotas == aceptaMascotas.Value).ToList();

            var total = propiedadList.Count;
            var paginated = propiedadList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                propiedades = paginated,
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
    /// POST: Crear nueva propiedad (requiere autenticación)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] PropiedadCreateDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var propiedad = await _propiedadService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = propiedad.Id }, propiedad);
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
    /// PUT: Actualizar propiedad (solo propietario)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] PropiedadUpdateDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var propiedad = await _propiedadService.UpdateAsync(id, dto, userId);
            return Ok(propiedad);
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
    /// DELETE: Eliminar propiedad (solo propietario)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _propiedadService.DeleteAsync(id, userId);
            if (!success)
                return BadRequest(new { error = "No se pudo eliminar la propiedad" });

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
    /// GET: Obtener mis propiedades (autenticado)
    /// </summary>
    [HttpGet("my-properties")]
    [Authorize]
    public async Task<IActionResult> GetMyProperties()
    {
        try
        {
            var userId = GetCurrentUserId();
            var propiedades = await _propiedadService.GetByPropietarioAsync(userId);
            return Ok(propiedades);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// POST: Subir fotos para una propiedad (solo propietario)
    /// </summary>
    [HttpPost("{id}/upload-photos")]
    [Authorize]
    public async Task<IActionResult> UploadPhotos(int id, [FromForm] List<IFormFile> files)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verificar que el usuario es propietario
            var propiedad = await _propiedadService.GetPropiedadForUpdateAsync(id);
            if (propiedad == null || propiedad.PropietarioId != userId)
                return Forbid();

            var uploadedUrls = new List<string>();
            
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // Crear nombre único para el archivo
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    
                    // Crear directorio si no existe
                    Directory.CreateDirectory(uploadsFolder);
                    
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    
                    uploadedUrls.Add($"/uploads/{fileName}");
                }
            }

            // Actualizar propiedad con nuevas fotos
            await _propiedadService.UploadPhotosAsync(id, uploadedUrls);
            
            return Ok(new { fotos = uploadedUrls });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Error al subir las fotos: {ex.Message}" });
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